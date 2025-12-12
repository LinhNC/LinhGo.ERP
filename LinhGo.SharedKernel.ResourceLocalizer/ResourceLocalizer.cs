using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinhGo.SharedKernel.ResourceLocalizer;

/// <summary>
/// Enhanced error message localizer using resource provider pattern
/// </summary>
internal class ResourceLocalizer(
    IOptions<ResourceLocalizerConfiguration> config,
    ILogger<ResourceLocalizer> logger)
    : IResourceLocalizer
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _localizedMessages = new();
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public string GetMessage(string localizeCode, params object[] args)
    {
        var currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        return GetMessage(localizeCode, currentLanguage, args);
    }

    public string GetMessage(string localizeCode, string languageCode, params object[]? args)
    {
        try
        {
            // Ensure messages are loaded for the requested language
            EnsureMessagesLoaded(languageCode);

            // Try to get message in requested language
            if (_localizedMessages.TryGetValue(languageCode, out var messages) &&
                messages.TryGetValue(localizeCode, out var message))
            {
                return FormatMessage(message, args);
            }

            var defaultLang = config.Value.DefaultLanguage;
            
            // Fallback to default language
            EnsureMessagesLoaded(defaultLang);
            if (_localizedMessages.TryGetValue(defaultLang, out var defaultMessages) &&
                defaultMessages.TryGetValue(localizeCode, out var defaultMessage))
            {
                logger.LogWarning(
                    "Message for code {ErrorCode} not found in language {Language}, using default",
                    localizeCode, languageCode);
                return FormatMessage(defaultMessage, args);
            }

            // If not found anywhere, return the error code itself
            logger.LogWarning("Message not found for code: {ErrorCode}", localizeCode);
            return localizeCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting message for code: {ErrorCode}", localizeCode);
            return localizeCode;
        }
    }
    
    private async Task<Dictionary<string, string>> LoadResourcesAsync(string languageCode)
    {
        try
        {
            var resourceFile = $"{languageCode}.json";

            var resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.Value.ResourcePath, resourceFile);

            if (!File.Exists(resourcePath))
            {
                logger.LogWarning("Resource file not found: {ResourcePath}", resourcePath);
                return new Dictionary<string, string>();
            }

            var json = await File.ReadAllTextAsync(resourcePath);
            var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            logger.LogInformation("Loaded {Count} error messages for language {Language}", 
                messages?.Count ?? 0, languageCode);
            
            return messages ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading error messages for language: {Language}", languageCode);
            return new Dictionary<string, string>();
        }
    }

    private void EnsureMessagesLoaded(string languageCode)
    {
        if (_localizedMessages.ContainsKey(languageCode))
        {
            return;
        }

        _loadLock.Wait();
        try
        {
            // Double-check after acquiring lock
            if (_localizedMessages.ContainsKey(languageCode))
            {
                return;
            }

            var messages = LoadResourcesAsync(languageCode).GetAwaiter().GetResult();
            _localizedMessages.TryAdd(languageCode, messages);

            logger.LogInformation(
                "Loaded {Count} error messages for language {Language}",
                messages.Count, languageCode);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private string FormatMessage(string message, object[]? args)
    {
        try
        {
            return args?.Length > 0 ? string.Format(message, args) : message;
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Error formatting message: {Message} with args: {Args}",
                message, string.Join(", ", args ?? Array.Empty<object>()));
            return message;
        }
    }
}


using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.Common.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinhGo.ERP.Application.Common.Localization;

/// <summary>
/// Enhanced error message localizer using resource provider pattern
/// </summary>
public class ResourceLocalizer(
    IOptions<ResourceLocalizerConfiguration> config,
    ILogger<ResourceLocalizer> logger)
    : IResourceLocalizer
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _localizedMessages = new();
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    
    private const string DefaultResourcePath = "Resources";
    private const string DefaultLocalizeFolder = "Localization";
    
    // Cache error codes to avoid repeated reflection - computed once and reused
    private static readonly Lazy<HashSet<string>> AllErrorCodes = new(GetAllErrorCodesFromConstants);

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

            // Fallback to default language
            EnsureMessagesLoaded(GeneralConstants.DefaultLanguage);
            if (_localizedMessages.TryGetValue(GeneralConstants.DefaultLanguage, out var defaultMessages) &&
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
    
    public async Task<Dictionary<string, string>> LoadResourcesAsync(string languageCode)
    {
        try
        {
            var resourceFile = $"{languageCode}.json";
            
            string resourcePath;
            if (config is not null && !string.IsNullOrEmpty(config.Value.ResourcePath))
            {
                resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.Value.ResourcePath, resourceFile);
            }
            else
            {
                resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultResourcePath, DefaultLocalizeFolder, resourceFile);
            }

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

            // Validate that all error codes have translations
            ValidateErrorCodes(languageCode, messages);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private void ValidateErrorCodes(string languageCode, Dictionary<string, string> messages)
    {
        // Use cached error codes - no repeated reflection calls
        var allErrorCodes = AllErrorCodes.Value;
        var missingCodes = allErrorCodes.Except(messages.Keys).ToList();

        if (missingCodes.Any())
        {
            logger.LogWarning(
                "Missing {Count} error code translations for language {Language}: {MissingCodes}",
                missingCodes.Count, languageCode, string.Join(", ", missingCodes));
        }
    }

    private static HashSet<string> GetAllErrorCodesFromConstants()
    {
        var errorCodes = new HashSet<string>();
        
        // Automatically discover all error classes in the same namespace as GeneralErrors
        var errorsNamespace = typeof(GeneralErrors).Namespace;
        
        var errorTypes = typeof(GeneralErrors).Assembly
            .GetTypes()
            .Where(t => t.Namespace == errorsNamespace
                        && t is { IsClass: true, IsPublic: true, IsAbstract: true, IsSealed: true })
            .ToList();

        foreach (var errorType in errorTypes)
        {
            var fields = errorType.GetFields(System.Reflection.BindingFlags.Public |
                                             System.Reflection.BindingFlags.Static |
                                             System.Reflection.BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    var value = field.GetValue(null)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        errorCodes.Add(value);
                    }
                }
            }
        }

        return errorCodes;
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

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResourceLocalizer(this IServiceCollection services, Action<ResourceLocalizerConfiguration>? resourceLocalizerConfiguration = null)
    {
        var configuration = new ResourceLocalizerConfiguration();

        if (resourceLocalizerConfiguration != null)
        {
            resourceLocalizerConfiguration.Invoke(configuration);
            services.Configure(resourceLocalizerConfiguration);
        }
        
        services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        
        return services;
    }
}

public class ResourceLocalizerConfiguration
{
    public string ResourcePath { get; set; }
}


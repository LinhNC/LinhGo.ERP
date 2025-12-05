using System.Collections.Concurrent;
using System.Globalization;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.Common.Errors;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Common.Localization;

/// <summary>
/// Enhanced error message localizer using resource provider pattern
/// </summary>
public class ErrorMessageLocalizer(
    ILogger<ErrorMessageLocalizer> logger,
    IErrorMessageResourceProvider resourceProvider)
    : IErrorMessageLocalizer
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _localizedMessages = new();
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    
    // Cache error codes to avoid repeated reflection - computed once and reused
    private static readonly Lazy<HashSet<string>> AllErrorCodes = new(GetAllErrorCodesFromConstants);

    public string GetErrorMessage(string errorCode, params object[] args)
    {
        var currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        return GetErrorMessage(errorCode, currentLanguage, args);
    }

    public string GetErrorMessage(string errorCode, string languageCode, params object[] args)
    {
        try
        {
            // Ensure messages are loaded for the requested language
            EnsureMessagesLoaded(languageCode);

            // Try to get message in requested language
            if (_localizedMessages.TryGetValue(languageCode, out var messages) &&
                messages.TryGetValue(errorCode, out var message))
            {
                return FormatMessage(message, args);
            }

            // Fallback to default language
            EnsureMessagesLoaded(GeneralConstants.DefaultLanguage);
            if (_localizedMessages.TryGetValue(GeneralConstants.DefaultLanguage, out var defaultMessages) &&
                defaultMessages.TryGetValue(errorCode, out var defaultMessage))
            {
                logger.LogWarning(
                    "Error message for code {ErrorCode} not found in language {Language}, using default",
                    errorCode, languageCode);
                return FormatMessage(defaultMessage, args);
            }

            // If not found anywhere, return the error code itself
            logger.LogWarning("Error message not found for code: {ErrorCode}", errorCode);
            return errorCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting error message for code: {ErrorCode}", errorCode);
            return errorCode;
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

            var messages = resourceProvider.LoadMessagesAsync(languageCode).GetAwaiter().GetResult();
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

    private string FormatMessage(string message, object[] args)
    {
        try
        {
            return args.Length > 0 ? string.Format(message, args) : message;
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Error formatting message: {Message} with args: {Args}",
                message, string.Join(", ", args));
            return message;
        }
    }
}


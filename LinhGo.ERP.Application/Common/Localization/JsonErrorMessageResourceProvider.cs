using System.Text.Json;
using LinhGo.ERP.Application.Common.Constants;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Common.Localization;

/// <summary>
/// Loads error messages from JSON resource files
/// </summary>
public class JsonErrorMessageResourceProvider(ILogger<JsonErrorMessageResourceProvider> logger)
    : IErrorMessageResourceProvider
{
    private const string ResourcePath = "Resources";
    private const string ErrorMessagesFolder = "Errors";

    public async Task<Dictionary<string, string>> LoadMessagesAsync(string languageCode)
    {
        try
        {
            var resourceFile = $"{languageCode}.json";
            var resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourcePath, ErrorMessagesFolder, resourceFile);

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

    public IEnumerable<string> GetSupportedLanguages()
    {
        return GeneralConstants.SupportedLanguages;
    }
}


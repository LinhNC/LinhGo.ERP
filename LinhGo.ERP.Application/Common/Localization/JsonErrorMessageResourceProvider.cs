using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Common.Localization;

/// <summary>
/// Loads error messages from JSON resource files
/// </summary>
public class JsonErrorMessageResourceProvider : IErrorMessageResourceProvider
{
    private readonly ILogger<JsonErrorMessageResourceProvider> _logger;
    private readonly string[] _supportedLanguages = { "en", "vi" };
    private const string ResourcePath = "Resources";
    private const string ErrorMessagesFolder = "Errors";

    public JsonErrorMessageResourceProvider(ILogger<JsonErrorMessageResourceProvider> logger)
    {
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> LoadMessagesAsync(string languageCode)
    {
        try
        {
            var resourceFile = $"{languageCode}.json";
            var resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourcePath, ErrorMessagesFolder, resourceFile);

            if (!File.Exists(resourcePath))
            {
                _logger.LogWarning("Resource file not found: {ResourcePath}", resourcePath);
                return new Dictionary<string, string>();
            }

            var json = await File.ReadAllTextAsync(resourcePath);
            var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            _logger.LogInformation("Loaded {Count} error messages for language {Language}", 
                messages?.Count ?? 0, languageCode);
            
            return messages ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading error messages for language: {Language}", languageCode);
            return new Dictionary<string, string>();
        }
    }

    public IEnumerable<string> GetSupportedLanguages()
    {
        return _supportedLanguages;
    }
}


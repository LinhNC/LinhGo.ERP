namespace LinhGo.ERP.Application.Common.Localization;

/// <summary>
/// Interface for loading error message resources from external sources
/// </summary>
public interface IErrorMessageResourceProvider
{
    /// <summary>
    /// Loads error messages for a specific language
    /// </summary>
    /// <param name="languageCode">The language code (e.g., "en", "vi")</param>
    /// <returns>Dictionary of error code to message mappings</returns>
    Task<Dictionary<string, string>> LoadMessagesAsync(string languageCode);
    
    /// <summary>
    /// Gets all supported language codes
    /// </summary>
    IEnumerable<string> GetSupportedLanguages();
}


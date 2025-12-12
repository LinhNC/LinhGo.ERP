namespace LinhGo.SharedKernel.ResourceLocalizer;

/// <summary>
/// Service for localizing messages
/// </summary>
public interface IResourceLocalizer
{
    /// <summary>
    /// Gets a localized message by localize code for current language
    /// </summary>
    /// <param name="localizeCode">The localize code</param>
    /// <param name="args">Optional arguments for string formatting</param>
    /// <returns>Localized error message</returns>
    string GetMessage(string localizeCode, params object[] args);
    
    /// <summary>
    /// Gets a localized message by localize code for a localize language
    /// </summary>
    /// <param name="localizeCode">The localize code</param>
    /// <param name="languageCode">The language code (e.g., "en", "vi")</param>
    /// <param name="args">Optional arguments for string formatting</param>
    /// <returns>Localized error message</returns>
    string GetMessage(string localizeCode, string languageCode, params object[]? args);
}


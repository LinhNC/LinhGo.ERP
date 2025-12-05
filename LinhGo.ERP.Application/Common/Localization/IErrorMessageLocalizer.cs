namespace LinhGo.ERP.Application.Common.Localization;

/// <summary>
/// Service for localizing error messages
/// </summary>
public interface IErrorMessageLocalizer
{
    /// <summary>
    /// Gets a localized error message by error code
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="args">Optional arguments for string formatting</param>
    /// <returns>Localized error message</returns>
    string GetErrorMessage(string errorCode, params object[] args);
    
    /// <summary>
    /// Gets a localized error message for a specific language
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="languageCode">The language code (e.g., "en", "vi")</param>
    /// <param name="args">Optional arguments for string formatting</param>
    /// <returns>Localized error message</returns>
    string GetErrorMessage(string errorCode, string languageCode, params object[] args);
}


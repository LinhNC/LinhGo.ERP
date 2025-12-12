namespace LinhGo.SharedKernel.ResourceLocalizer;

public class ResourceLocalizerConfiguration
{
    /// <summary>
    /// Complete path to the folder containing localization resource files
    /// Defaults to "Resources/Localization"
    /// </summary>
    public string ResourcePath { get; set; } = Path.Combine("Resources", "Localization");
    
    /// <summary>
    /// Set the default language code to fall back to
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";
}
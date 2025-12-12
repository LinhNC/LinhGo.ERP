namespace LinhGo.SharedKernel.Api.Configurations;

internal class CorsPolicySettings
{
    public string[] Domains { get; set; } = Array.Empty<string>();
}
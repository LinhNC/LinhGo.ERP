using System.ComponentModel.DataAnnotations;

namespace LinhGo.SharedKernel.Api.Configurations;

internal class ServiceConfigurations
{
    [Required] 
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
    
    [Required] 
    public CorsPolicySettings CorsPolicySettings { get; set; } = null!;
}
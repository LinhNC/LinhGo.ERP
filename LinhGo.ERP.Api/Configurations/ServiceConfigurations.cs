using System.ComponentModel.DataAnnotations;

namespace LinhGo.ERP.Api.Configurations;

public class ServiceConfigurations
{
    [Required] 
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
    
    [Required] 
    public CorsPolicySettings CorsPolicySettings { get; set; } = null!;
}
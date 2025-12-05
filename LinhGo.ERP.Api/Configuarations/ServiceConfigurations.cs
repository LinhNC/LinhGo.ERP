using System.ComponentModel.DataAnnotations;

namespace LinhGo.ERP.Api.Configuarations;

public class ServiceConfigurations
{
    [Required] 
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
    
    [Required] 
    public CorsPolicySettings CorsPolicySettings { get; set; } = null!;
}
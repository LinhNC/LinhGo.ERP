namespace LinhGo.ERP.Application.DTOs.Customers;

public class CreateCustomerDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Type { get; set; } = "Individual";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; } = 30;
}


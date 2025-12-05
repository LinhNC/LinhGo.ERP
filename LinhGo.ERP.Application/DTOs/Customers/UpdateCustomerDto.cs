namespace LinhGo.ERP.Application.DTOs.Customers;

public class UpdateCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
}


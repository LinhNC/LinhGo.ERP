namespace LinhGo.ERP.Application.DTOs.Customers;

public class CustomerAddressDto
{
    public Guid Id { get; set; }
    public string AddressType { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
}


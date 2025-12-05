namespace LinhGo.ERP.Application.DTOs.Customers;

public class CustomerDetailsDto : CustomerDto
{
    public List<CustomerContactDto> Contacts { get; set; } = new();
    public List<CustomerAddressDto> Addresses { get; set; } = new();
}


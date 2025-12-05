# Application Layer File Generator
$ErrorActionPreference = "Stop"
$filesCreated = 0
Write-Host "Creating Application Layer Files..." -ForegroundColor Cyan
# Create Customers DTOs
$customerDtos = @"
namespace LinhGo.ERP.Application.DTOs.Customers;
public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public int PaymentTermDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class CustomerDetailsDto : CustomerDto
{
    public List<CustomerContactDto> Contacts { get; set; } = new();
    public List<CustomerAddressDto> Addresses { get; set; } = new();
}
public class CustomerContactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
}
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
public class UpdateCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
}
"@
$dir = "E:\Projects\NET\LinhGo.ERP\LinhGo.ERP.Application\DTOs\Customers"
if (!(Test-Path $dir)) { New-Item -Path $dir -ItemType Directory -Force | Out-Null }
Set-Content -Path "$dir\CustomerDtos.cs" -Value $customerDtos -Encoding UTF8
$filesCreated++
Write-Host "  [+] CustomerDtos.cs" -ForegroundColor Green
Write-Host "`nFiles created: $filesCreated" -ForegroundColor Cyan

# Application Layer Generator Script
$ErrorActionPreference = "Stop"
$baseDir = "E:\Projects\NET\LinhGo.ERP\LinhGo.ERP.Application"
$filesCreated = 0

Write-Host "=== Application Layer File Generator ===" -ForegroundColor Cyan
Write-Host ""

# Customer Service
$customerService = @'
using AutoMapper;
using LinhGo.ERP.Application.Common;
using LinhGo.ERP.Application.DTOs.Customers;
using LinhGo.ERP.Domain.Customers.Entities;
using LinhGo.ERP.Domain.Customers.Interfaces;

namespace LinhGo.ERP.Application.Services;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetByIdAsync(Guid companyId, Guid id);
    Task<Result<CustomerDetailsDto>> GetDetailsAsync(Guid companyId, Guid id);
    Task<Result<PagedResult<CustomerDto>>> GetPagedAsync(Guid companyId, int page, int pageSize);
    Task<Result<IEnumerable<CustomerDto>>> SearchAsync(Guid companyId, string searchTerm);
    Task<Result<CustomerDto>> CreateAsync(Guid companyId, CreateCustomerDto dto);
    Task<Result<CustomerDto>> UpdateAsync(Guid companyId, UpdateCustomerDto dto);
    Task<Result> DeleteAsync(Guid companyId, Guid id);
}

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid companyId, Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(companyId, id);
        if (customer == null) return Result<CustomerDto>.FailureResult($"Customer with ID {id} not found");
        return Result<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<CustomerDetailsDto>> GetDetailsAsync(Guid companyId, Guid id)
    {
        var customer = await _customerRepository.GetWithDetailsAsync(companyId, id);
        if (customer == null) return Result<CustomerDetailsDto>.FailureResult($"Customer with ID {id} not found");
        return Result<CustomerDetailsDto>.SuccessResult(_mapper.Map<CustomerDetailsDto>(customer));
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetPagedAsync(Guid companyId, int page, int pageSize)
    {
        var customers = await _customerRepository.GetPagedAsync(companyId, page, pageSize);
        var totalCount = await _customerRepository.CountAsync(companyId);
        var pagedResult = new PagedResult<CustomerDto>
        {
            Items = _mapper.Map<IEnumerable<CustomerDto>>(customers),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
        return Result<PagedResult<CustomerDto>>.SuccessResult(pagedResult);
    }

    public async Task<Result<IEnumerable<CustomerDto>>> SearchAsync(Guid companyId, string searchTerm)
    {
        var customers = await _customerRepository.SearchCustomersAsync(companyId, searchTerm);
        return Result<IEnumerable<CustomerDto>>.SuccessResult(_mapper.Map<IEnumerable<CustomerDto>>(customers));
    }

    public async Task<Result<CustomerDto>> CreateAsync(Guid companyId, CreateCustomerDto dto)
    {
        var isUnique = await _customerRepository.IsCodeUniqueAsync(companyId, dto.Code);
        if (!isUnique) return Result<CustomerDto>.FailureResult($"Customer code '{dto.Code}' already exists");
        var customer = _mapper.Map<Customer>(dto);
        customer = await _customerRepository.AddAsync(companyId, customer);
        return Result<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(customer), "Customer created successfully");
    }

    public async Task<Result<CustomerDto>> UpdateAsync(Guid companyId, UpdateCustomerDto dto)
    {
        var existing = await _customerRepository.GetByIdAsync(companyId, dto.Id);
        if (existing == null) return Result<CustomerDto>.FailureResult($"Customer with ID {dto.Id} not found");
        _mapper.Map(dto, existing);
        await _customerRepository.UpdateAsync(companyId, existing);
        return Result<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(existing), "Customer updated successfully");
    }

    public async Task<Result> DeleteAsync(Guid companyId, Guid id)
    {
        var existing = await _customerRepository.GetByIdAsync(companyId, id);
        if (existing == null) return Result.FailureResult($"Customer with ID {id} not found");
        await _customerRepository.DeleteAsync(companyId, id);
        return Result.SuccessResult("Customer deleted successfully");
    }
}
'@

[System.IO.File]::WriteAllText("$baseDir\Services\CustomerService.cs", $customerService, [System.Text.Encoding]::UTF8)
$filesCreated++
Write-Host "[+] CustomerService.cs" -ForegroundColor Green

Write-Host ""
Write-Host "Total files created: $filesCreated" -ForegroundColor Cyan
Write-Host "Script completed successfully!" -ForegroundColor Green


using LinhGo.ERP.Application.DTOs.Companies;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;

namespace LinhGo.ERP.Application.Abstractions.Services;

public interface ICompanyService
{
    // CRUD Operations
    Task<Result<CompanyDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<CompanyDto>>> GetAllAsync();
    Task<Result<IEnumerable<CompanyDto>>> GetActiveCompaniesAsync();
    Task<Result<CompanyDto>> GetByCodeAsync(string code);
    Task<Result<CompanyDto>> CreateAsync(CreateCompanyDto dto);
    Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<PagedResult<CompanyDto>>> QueryAsync(QuerierParams queries, CancellationToken ctx);
}
﻿using LinhGo.ERP.Application.Common;
using LinhGo.ERP.Application.Common.SearchBuilders;
using LinhGo.ERP.Application.DTOs.Companies;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Companies.Entities;

namespace LinhGo.ERP.Application.Abstractions.Services;

public interface ICompanyService
{
    Task<Result<CompanyDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<CompanyDto>>> GetAllAsync();
    Task<Result<IEnumerable<CompanyDto>>> GetActiveCompaniesAsync();
    Task<Result<CompanyDto>> GetByCodeAsync(string code);
    Task<Result<CompanyDto>> CreateAsync(CreateCompanyDto dto);
    Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<PagedResult<CompanyDto>>> SearchAsync(SearchQueryParams queries, CancellationToken ctx);
}
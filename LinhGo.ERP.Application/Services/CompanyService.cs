using AutoMapper;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Errors;
using LinhGo.ERP.Application.DTOs.Companies;
using LinhGo.ERP.Domain.Common;
using LinhGo.ERP.Domain.Companies.Entities;
using LinhGo.ERP.Domain.Companies.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinhGo.ERP.Application.Services;

public class CompanyService(
    ICompanyRepository companyRepository,
    IMapper mapper,
    ILogger<CompanyService> logger) : ICompanyService
{
    public async Task<Result<CompanyDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var company = await companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                logger.LogWarning("Company with ID {CompanyId} not found", id);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, id);
            }

            var result = mapper.Map<CompanyDto>(company);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving company with ID {CompanyId}", id);
            return Error.WithFailureCode(CompanyErrors.GetByIdFailed);
        }
       
    }
    public async Task<Result<IEnumerable<CompanyDto>>> GetAllAsync()
    {
        try
        {
            var companies = await companyRepository.GetAllAsync();
            var result = mapper.Map<IEnumerable<CompanyDto>>(companies);
            return result?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all companies");
            return Error.WithFailureCode(CompanyErrors.GetAllFailed);
        }
    }

    public async Task<Result<IEnumerable<CompanyDto>>> GetActiveCompaniesAsync()
    {
        try
        {
            var companies = await companyRepository.GetActiveCompaniesAsync();
            IEnumerable<CompanyDto> result = mapper.Map<IEnumerable<CompanyDto>>(companies);
            return result?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active companies");
            return Error.WithFailureCode(CompanyErrors.GetActiveFailed);
        }
    }

    public async Task<Result<CompanyDto>> GetByCodeAsync(string code)
    {
        try
        {
            var company = await companyRepository.GetByCodeAsync(code);
            if (company == null)
            {
                logger.LogWarning("Company with code {Code} not found", code);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, code);
            }

            var result = mapper.Map<CompanyDto>(company);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving company with code {Code}", code);
            return Error.WithFailureCode(CompanyErrors.GetByCodeFailed);
        }
    }

    public async Task<Result<CompanyDto>> CreateAsync(CreateCompanyDto dto)
    {
        try
        {
            var isUnique = await companyRepository.IsCodeUniqueAsync(dto.Code);
            if (!isUnique)
            {
                logger.LogWarning("Attempt to create duplicate company with code {Code}", dto.Code);
                return Error.WithConflictCode(CompanyErrors.DuplicateCode, dto.Code);
            }

            var company = mapper.Map<Company>(dto);
            company = await companyRepository.AddAsync(company);
            
            var result = mapper.Map<CompanyDto>(company);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error attempting to create company with code {Code}", dto.Code);
            return Error.WithFailureCode(CompanyErrors.CreateFailed);
        }
    }

    public async Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                logger.LogWarning("Mismatched company ID in update request: {RouteId} vs {DtoId}", id, dto.Id);
                return Error.WithValidationCode(CompanyErrors.IdMismatch, id, dto.Id);
            }
            
            // Fetch existing entity from database
            var existing = await companyRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to update non-existent company with ID {CompanyId}", dto.Id);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, dto.Id);
            }

            // Map DTO to existing entity (preserves Code and other unmapped fields)
            // Version from DTO will be used for concurrency check
            mapper.Map(dto, existing);
            
            // Update entity - will throw DbUpdateConcurrencyException if Version doesn't match database
            await companyRepository.UpdateAsync(existing);
            
            var result = mapper.Map<CompanyDto>(existing);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Concurrency conflict: Entity was modified by another user
            logger.LogWarning(ex, "Concurrency conflict updating company with ID {CompanyId}", dto.Id);
            return Error.WithConflictCode(CompanyErrors.ConcurrencyConflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error attempting to update company with ID {CompanyId}", dto.Id);
            return Error.WithFailureCode(CompanyErrors.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Deleting company {CompanyId}", id);
            
            var existing = await companyRepository.GetByIdAsync(id);
            if (existing == null)
            {
                logger.LogWarning("Attempt to delete non-existent company with ID {CompanyId}", id);
                return Error.WithNotFoundCode(CompanyErrors.NotFound, id);
            }

            await companyRepository.DeleteAsync(id);
            logger.LogInformation("Company deleted successfully with ID {CompanyId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting company with ID {CompanyId}", id);
            return Error.WithFailureCode(CompanyErrors.DeleteFailed);
        }
    }

    public async Task<Result<PagedResult<CompanyDto>>> SearchAsync(SearchQueryParams queries, CancellationToken ctx)
    {
        try
        {
            var result = await companyRepository.SearchAsync(queries, ctx);
            var mappedResult = new PagedResult<CompanyDto>
            {
                Items = mapper.Map<IEnumerable<CompanyDto>>(result.Items),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching companies with queries {@Queries}", queries);
            return Error.WithFailureCode(CompanyErrors.SearchFailed);
        }
    }
}

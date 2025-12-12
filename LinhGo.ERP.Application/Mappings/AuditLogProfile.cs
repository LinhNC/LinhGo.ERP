using AutoMapper;
using LinhGo.ERP.Application.DTOs.Audit;
using LinhGo.ERP.Domain.Audit.Entities;

namespace LinhGo.ERP.Application.Mappings;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<AuditLog, AuditLogDetailDto>();
    }
}


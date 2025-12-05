using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Users.Entities;
namespace LinhGo.ERP.Infrastructure.Data.Configurations;
public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.HasKey(uc => uc.Id);
        builder.HasIndex(uc => new { uc.UserId, uc.CompanyId }).IsUnique();
        builder.HasIndex(uc => uc.CompanyId);
        builder.Property(uc => uc.Role).IsRequired().HasMaxLength(50);
        builder.HasOne(uc => uc.User).WithMany(u => u.UserCompanies).HasForeignKey(uc => uc.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(uc => uc.Company).WithMany().HasForeignKey(uc => uc.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(uc => uc.Permissions).WithOne(p => p.UserCompany).HasForeignKey(p => p.UserCompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}

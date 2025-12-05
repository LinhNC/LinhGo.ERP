using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Users.Entities;
namespace LinhGo.ERP.Infrastructure.Data.Configurations;
public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(up => up.Id);
        builder.HasIndex(up => new { up.UserCompanyId, up.PermissionKey }).IsUnique();
        builder.Property(up => up.PermissionKey).IsRequired().HasMaxLength(100);
        builder.HasOne(up => up.UserCompany).WithMany(uc => uc.Permissions).HasForeignKey(up => up.UserCompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}

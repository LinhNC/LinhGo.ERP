using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Companies.Entities;
namespace LinhGo.ERP.Infrastructure.Data.Configurations;
public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.HasKey(cs => cs.Id);
        builder.HasIndex(cs => new { cs.CompanyId, cs.SettingKey }).IsUnique();
        builder.Property(cs => cs.SettingKey).IsRequired().HasMaxLength(100);
        builder.Property(cs => cs.SettingValue).IsRequired().HasMaxLength(1000);
        builder.Property(cs => cs.SettingGroup).HasMaxLength(50);
        builder.Property(cs => cs.Description).HasMaxLength(500);
        builder.HasOne(cs => cs.Company).WithMany(c => c.Settings).HasForeignKey(cs => cs.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}

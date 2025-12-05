using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.HasIndex(pc => pc.CompanyId);
        builder.HasIndex(pc => new { pc.CompanyId, pc.Code }).IsUnique();
        builder.HasIndex(pc => pc.ParentCategoryId);

        builder.Property(pc => pc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pc => pc.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pc => pc.Description)
            .HasMaxLength(500);

        builder.Property(pc => pc.Path)
            .HasMaxLength(500);

        // Self-referencing relationship
        builder.HasOne(pc => pc.ParentCategory)
            .WithMany(pc => pc.SubCategories)
            .HasForeignKey(pc => pc.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Products relationship
        builder.HasMany(pc => pc.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


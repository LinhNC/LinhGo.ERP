using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(pv => pv.Id);

        builder.HasIndex(pv => pv.CompanyId);
        builder.HasIndex(pv => pv.ProductId);
        builder.HasIndex(pv => new { pv.CompanyId, pv.Code }).IsUnique();

        builder.Property(pv => pv.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pv => pv.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pv => pv.Size)
            .HasMaxLength(50);

        builder.Property(pv => pv.Color)
            .HasMaxLength(50);

        builder.Property(pv => pv.Style)
            .HasMaxLength(50);

        builder.Property(pv => pv.Attributes)
            .HasMaxLength(500);

        builder.Property(pv => pv.Barcode)
            .HasMaxLength(100);

        builder.Property(pv => pv.PriceAdjustment)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.CostAdjustment)
            .HasPrecision(18, 2);

        // Relationship
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


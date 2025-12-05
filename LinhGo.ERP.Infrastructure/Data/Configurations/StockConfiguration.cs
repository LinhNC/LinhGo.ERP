using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => new { s.CompanyId, s.ProductId, s.WarehouseId }).IsUnique();
        builder.HasIndex(s => s.ProductId);
        builder.HasIndex(s => s.WarehouseId);

        builder.Property(s => s.QuantityOnHand)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityReserved)
            .HasPrecision(18, 4);

        builder.Property(s => s.AverageCost)
            .HasPrecision(18, 4);

        // Computed column - not mapped, calculated in entity
        builder.Ignore(s => s.QuantityAvailable);
        builder.Ignore(s => s.TotalValue);

        // Relationships
        builder.HasOne(s => s.Product)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ProductVariant)
            .WithMany()
            .HasForeignKey(s => s.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.Stocks)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


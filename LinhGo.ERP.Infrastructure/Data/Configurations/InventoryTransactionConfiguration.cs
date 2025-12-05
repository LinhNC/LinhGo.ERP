using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(it => it.Id);

        builder.HasIndex(it => it.CompanyId);
        builder.HasIndex(it => new { it.CompanyId, it.TransactionNumber }).IsUnique();
        builder.HasIndex(it => it.ProductId);
        builder.HasIndex(it => it.TransactionDate);

        builder.Property(it => it.TransactionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(it => it.Quantity)
            .HasPrecision(18, 4);

        builder.Property(it => it.UnitCost)
            .HasPrecision(18, 4);

        // Computed column
        builder.Ignore(it => it.TotalCost);

        builder.Property(it => it.ReferenceType)
            .HasMaxLength(50);

        builder.Property(it => it.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(it => it.Notes)
            .HasMaxLength(500);

        builder.Property(it => it.Reason)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(it => it.Product)
            .WithMany(p => p.Transactions)
            .HasForeignKey(it => it.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(it => it.ProductVariant)
            .WithMany()
            .HasForeignKey(it => it.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(it => it.FromWarehouse)
            .WithMany()
            .HasForeignKey(it => it.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(it => it.ToWarehouse)
            .WithMany()
            .HasForeignKey(it => it.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


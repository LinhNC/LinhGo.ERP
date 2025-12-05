using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Orders.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.HasIndex(oi => oi.CompanyId);
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);

        builder.Property(oi => oi.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.ProductDescription)
            .HasMaxLength(500);

        builder.Property(oi => oi.VariantName)
            .HasMaxLength(200);

        builder.Property(oi => oi.Quantity)
            .HasPrecision(18, 4);

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.Property(oi => oi.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TaxPercentage)
            .HasPrecision(5, 2);

        builder.Property(oi => oi.LineTotal)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.QuantityShipped)
            .HasPrecision(18, 4);

        builder.Property(oi => oi.QuantityReturned)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.Warehouse)
            .WithMany()
            .HasForeignKey(oi => oi.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


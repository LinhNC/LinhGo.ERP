using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Orders.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class OrderShipmentConfiguration : IEntityTypeConfiguration<OrderShipment>
{
    public void Configure(EntityTypeBuilder<OrderShipment> builder)
    {
        builder.HasKey(os => os.Id);

        builder.HasIndex(os => os.CompanyId);
        builder.HasIndex(os => new { os.CompanyId, os.ShipmentNumber }).IsUnique();
        builder.HasIndex(os => os.OrderId);
        builder.HasIndex(os => os.ShipmentDate);
        builder.HasIndex(os => os.Status);

        builder.Property(os => os.ShipmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(os => os.Carrier)
            .HasMaxLength(100);

        builder.Property(os => os.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(os => os.ShippingMethod)
            .HasMaxLength(100);

        builder.Property(os => os.ShippingCost)
            .HasPrecision(18, 2);

        builder.Property(os => os.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(os => os.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(os => os.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(os => os.Items)
            .WithOne(i => i.Shipment)
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

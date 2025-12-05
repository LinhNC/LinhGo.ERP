using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Orders.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class OrderShipmentItemConfiguration : IEntityTypeConfiguration<OrderShipmentItem>
{
    public void Configure(EntityTypeBuilder<OrderShipmentItem> builder)
    {
        builder.HasKey(osi => osi.Id);

        builder.HasIndex(osi => osi.CompanyId);
        builder.HasIndex(osi => osi.ShipmentId);
        builder.HasIndex(osi => osi.OrderItemId);

        builder.Property(osi => osi.Quantity)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(osi => osi.Shipment)
            .WithMany(os => os.Items)
            .HasForeignKey(osi => osi.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(osi => osi.OrderItem)
            .WithMany()
            .HasForeignKey(osi => osi.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


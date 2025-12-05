using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Orders.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.HasKey(op => op.Id);

        builder.HasIndex(op => op.CompanyId);
        builder.HasIndex(op => op.OrderId);
        builder.HasIndex(op => new { op.CompanyId, op.PaymentNumber }).IsUnique();

        builder.Property(op => op.PaymentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(op => op.Amount)
            .HasPrecision(18, 2);

        builder.Property(op => op.Currency)
            .HasMaxLength(3);

        builder.Property(op => op.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(op => op.PaymentReference)
            .HasMaxLength(200);

        builder.Property(op => op.Notes)
            .HasMaxLength(500);

        // Relationship
        builder.HasOne(op => op.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(op => op.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


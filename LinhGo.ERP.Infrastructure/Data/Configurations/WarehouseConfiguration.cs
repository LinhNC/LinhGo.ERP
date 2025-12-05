using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Inventory.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.CompanyId);
        builder.HasIndex(w => new { w.CompanyId, w.Code }).IsUnique();

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(500);

        builder.Property(w => w.AddressLine1)
            .HasMaxLength(200);

        builder.Property(w => w.AddressLine2)
            .HasMaxLength(200);

        builder.Property(w => w.City)
            .HasMaxLength(100);

        builder.Property(w => w.State)
            .HasMaxLength(100);

        builder.Property(w => w.PostalCode)
            .HasMaxLength(20);

        builder.Property(w => w.Country)
            .HasMaxLength(100);

        builder.Property(w => w.ContactPerson)
            .HasMaxLength(100);

        builder.Property(w => w.Phone)
            .HasMaxLength(20);

        builder.Property(w => w.Email)
            .HasMaxLength(200);

        // Relationships
        builder.HasMany(w => w.Stocks)
            .WithOne(s => s.Warehouse)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


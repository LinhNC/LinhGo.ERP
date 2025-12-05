using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Customers.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasKey(ca => ca.Id);

        builder.HasIndex(ca => ca.CompanyId);
        builder.HasIndex(ca => ca.CustomerId);

        builder.Property(ca => ca.AddressType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ca => ca.Label)
            .HasMaxLength(100);

        builder.Property(ca => ca.AddressLine1)
            .HasMaxLength(200);

        builder.Property(ca => ca.AddressLine2)
            .HasMaxLength(200);

        builder.Property(ca => ca.City)
            .HasMaxLength(100);

        builder.Property(ca => ca.State)
            .HasMaxLength(100);

        builder.Property(ca => ca.PostalCode)
            .HasMaxLength(20);

        builder.Property(ca => ca.Country)
            .HasMaxLength(100);

        // Relationship
        builder.HasOne(ca => ca.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


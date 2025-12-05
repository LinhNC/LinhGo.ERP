using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinhGo.ERP.Domain.Customers.Entities;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.HasKey(cc => cc.Id);

        builder.HasIndex(cc => cc.CompanyId);
        builder.HasIndex(cc => cc.CustomerId);

        builder.Property(cc => cc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cc => cc.Position)
            .HasMaxLength(100);

        builder.Property(cc => cc.Email)
            .HasMaxLength(200);

        builder.Property(cc => cc.Phone)
            .HasMaxLength(20);

        builder.Property(cc => cc.Mobile)
            .HasMaxLength(20);

        // Relationship
        builder.HasOne(cc => cc.Customer)
            .WithMany(c => c.Contacts)
            .HasForeignKey(cc => cc.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


using LinhGo.ERP.Domain.Audit.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinhGo.ERP.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.UserId)
            .HasMaxLength(100);

        builder.Property(a => a.UserName)
            .HasMaxLength(200);

        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for efficient querying

        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for efficient querying

        builder.Property(a => a.AffectedColumns)
            .HasMaxLength(1000);

        builder.Property(a => a.PrimaryKey)
            .HasMaxLength(100);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        // Indexes for efficient querying
        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.CompanyId);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}


using Microsoft.EntityFrameworkCore;
using QuantumShield.Be.Infrastructure.Persistence.Entities;

namespace QuantumShield.Be.Infrastructure.Persistence;

public sealed class ZeroTrustDbContext : DbContext
{
    public ZeroTrustDbContext(DbContextOptions<ZeroTrustDbContext> options)
        : base(options)
    {
    }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

    public DbSet<EvaluationRunEntity> EvaluationRuns => Set<EvaluationRunEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantEntity>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TenantName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.ClientId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.SecretReference).HasMaxLength(500).IsRequired();
            entity.Property(item => item.IsB2C).IsRequired();
            entity.Property(item => item.CreatedAtUtc).IsRequired();
            entity.Property(item => item.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<EvaluationRunEntity>(entity =>
        {
            entity.ToTable("EvaluationRuns");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ResultBlobName).HasMaxLength(1000);
            entity.HasOne(item => item.Tenant)
                .WithMany()
                .HasForeignKey(item => item.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

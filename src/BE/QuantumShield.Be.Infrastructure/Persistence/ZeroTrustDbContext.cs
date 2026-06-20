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

    public DbSet<EvaluationResultEntity> EvaluationResults => Set<EvaluationResultEntity>();

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
            entity.Property(item => item.CreatedAtUtc).IsRequired();
            entity.Property(item => item.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<EvaluationRunEntity>(entity =>
        {
            entity.ToTable("EvaluationRuns");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TemplateIdentifier).HasMaxLength(255).IsRequired();
            entity.Property(item => item.TemplateVersion).HasMaxLength(100);
            entity.Property(item => item.ErrorMessage).HasMaxLength(4000);
            entity.HasOne(item => item.Tenant)
                .WithMany()
                .HasForeignKey(item => item.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(item => item.Results)
                .WithOne(item => item.Run)
                .HasForeignKey(item => item.EvaluationRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EvaluationResultEntity>(entity =>
        {
            entity.ToTable("EvaluationResults");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.RuleKey).HasMaxLength(255).IsRequired();
            entity.Property(item => item.DisplayName).HasMaxLength(255).IsRequired();
            entity.Property(item => item.ExpectedValue).HasMaxLength(4000);
            entity.Property(item => item.ActualValue).HasMaxLength(4000);
            entity.Property(item => item.Notes).HasMaxLength(4000);
        });
    }
}

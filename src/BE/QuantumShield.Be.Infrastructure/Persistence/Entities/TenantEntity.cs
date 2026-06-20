namespace QuantumShield.Be.Infrastructure.Persistence.Entities;

public sealed class TenantEntity
{
    public Guid Id { get; set; }

    public string TenantName { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string SecretReference { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

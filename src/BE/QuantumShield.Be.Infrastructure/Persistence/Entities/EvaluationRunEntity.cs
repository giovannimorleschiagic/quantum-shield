using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Infrastructure.Persistence.Entities;

public sealed class EvaluationRunEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public TenantEntity? Tenant { get; set; }

    public EvaluationRunStatus Status { get; set; }

    public string? ResultBlobName { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }
}

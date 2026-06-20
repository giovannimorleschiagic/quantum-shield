using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Infrastructure.Persistence.Entities;

public sealed class EvaluationRunEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public TenantEntity? Tenant { get; set; }

    public EvaluationRunStatus Status { get; set; }

    public string TemplateIdentifier { get; set; } = string.Empty;

    public string? TemplateVersion { get; set; }

    public int TotalChecks { get; set; }

    public int PassedChecks { get; set; }

    public int FailedChecks { get; set; }

    public int NotApplicableChecks { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public ICollection<EvaluationResultEntity> Results { get; set; } = [];
}

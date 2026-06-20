using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Infrastructure.Persistence.Entities;

public sealed class EvaluationResultEntity
{
    public Guid Id { get; set; }

    public Guid EvaluationRunId { get; set; }

    public EvaluationRunEntity? Run { get; set; }

    public string RuleKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public EvaluationCheckStatus Status { get; set; }

    public EvaluationSeverity Severity { get; set; }

    public string? ExpectedValue { get; set; }

    public string? ActualValue { get; set; }

    public string? Notes { get; set; }
}

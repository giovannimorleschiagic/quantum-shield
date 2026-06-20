using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationResult
{
    private EvaluationResult()
    {
    }

    public EvaluationResult(
        string ruleKey,
        string displayName,
        EvaluationCheckStatus status,
        EvaluationSeverity severity,
        string? expectedValue,
        string? actualValue,
        string? notes)
    {
        Id = Guid.NewGuid();
        RuleKey = ruleKey;
        DisplayName = displayName;
        Status = status;
        Severity = severity;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
        Notes = notes;
    }

    public Guid Id { get; private set; }

    public string RuleKey { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public EvaluationCheckStatus Status { get; private set; }

    public EvaluationSeverity Severity { get; private set; }

    public string? ExpectedValue { get; private set; }

    public string? ActualValue { get; private set; }

    public string? Notes { get; private set; }
}

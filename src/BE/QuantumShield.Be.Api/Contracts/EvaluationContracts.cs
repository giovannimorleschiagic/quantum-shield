using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Api.Contracts;

public sealed record TriggerEvaluationRunRequest(Guid TenantId, string? TemplateIdentifier);

public sealed record EvaluationResultResponse(
    Guid Id,
    string RuleKey,
    string DisplayName,
    EvaluationCheckStatus Status,
    EvaluationSeverity Severity,
    string? ExpectedValue,
    string? ActualValue,
    string? Notes);

public sealed record EvaluationRunResponse(
    Guid Id,
    Guid TenantId,
    EvaluationRunStatus Status,
    string TemplateIdentifier,
    string? TemplateVersion,
    int TotalChecks,
    int PassedChecks,
    int FailedChecks,
    int NotApplicableChecks,
    string? ErrorMessage,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<EvaluationResultResponse> Results);

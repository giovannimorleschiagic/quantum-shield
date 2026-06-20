using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Domain.Models;

public sealed record EvaluationArtifactDocument(
    Guid EvaluationId,
    Guid TenantId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    EvaluationRunStatus Status,
    EvaluationArtifactSummary Summary,
    IReadOnlyCollection<EvaluationArtifactTemplateResult> Templates);

public sealed record EvaluationArtifactSummary(
    int TotalChecks,
    int PassedChecks,
    int FailedChecks,
    int NotApplicableChecks,
    int TemplatesProcessed,
    int TemplatesSkipped);

public sealed record EvaluationArtifactTemplateResult(
    string ControlId,
    string Benchmark,
    string? Version,
    string Section,
    string Title,
    IReadOnlyCollection<EvaluationCheckResult> Checks);

public sealed record EvaluationCheckResult(
    string ControlId,
    string CheckId,
    string Title,
    string Description,
    string Method,
    string Endpoint,
    IReadOnlyCollection<string> GraphPermissions,
    string ExpectedResult,
    EvaluationCheckStatus Status,
    string? ActualResult,
    string? RawResult,
    string? Notes);

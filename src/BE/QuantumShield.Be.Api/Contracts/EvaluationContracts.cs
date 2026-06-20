using QuantumShield.Be.Domain.Enums;

namespace QuantumShield.Be.Api.Contracts;

public sealed record TriggerEvaluationRunRequest(Guid TenantId);

public sealed record EvaluationRunSummaryResponse(
    Guid Id,
    Guid TenantId,
    EvaluationRunStatus Status,
    string? ResultBlobName,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc);

public sealed record EvaluationArtifactSummaryResponse(
    int TotalChecks,
    int PassedChecks,
    int FailedChecks,
    int NotApplicableChecks,
    int TemplatesProcessed,
    int TemplatesSkipped);

public sealed record EvaluationCheckResultResponse(
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

public sealed record EvaluationTemplateResultResponse(
    string ControlId,
    string Benchmark,
    string? Version,
    string Section,
    string Title,
    IReadOnlyCollection<EvaluationCheckResultResponse> Checks);

public sealed record EvaluationRunDetailResponse(
    Guid Id,
    Guid TenantId,
    EvaluationRunStatus Status,
    string? ResultBlobName,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    EvaluationArtifactSummaryResponse? Summary,
    IReadOnlyCollection<EvaluationTemplateResultResponse> Templates);

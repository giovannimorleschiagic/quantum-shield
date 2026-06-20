using QuantumShield.Be.Api.Contracts;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Api;

internal static class ApiMappings
{
    public static TenantResponse ToResponse(this Tenant tenant)
        => new(
            tenant.Id,
            tenant.TenantName,
            tenant.TenantId,
            tenant.ClientId,
            tenant.SecretReference,
            tenant.IsActive,
            tenant.CreatedAtUtc,
            tenant.UpdatedAtUtc);

    public static EvaluationRunSummaryResponse ToSummaryResponse(this EvaluationRun run)
        => new(
            run.Id,
            run.TenantId,
            run.Status,
            run.ResultBlobName,
            run.StartedAtUtc,
            run.CompletedAtUtc);

    public static EvaluationRunDetailResponse ToDetailResponse(this EvaluationRun run)
        => new(
            run.Id,
            run.TenantId,
            run.Status,
            run.ResultBlobName,
            run.StartedAtUtc,
            run.CompletedAtUtc,
            run.Artifact is null
                ? null
                : new EvaluationArtifactSummaryResponse(
                    run.Artifact.Summary.TotalChecks,
                    run.Artifact.Summary.PassedChecks,
                    run.Artifact.Summary.FailedChecks,
                    run.Artifact.Summary.NotApplicableChecks,
                    run.Artifact.Summary.TemplatesProcessed,
                    run.Artifact.Summary.TemplatesSkipped),
            run.Artifact?.Templates.Select(static template => new EvaluationTemplateResultResponse(
                    template.ControlId,
                    template.Benchmark,
                    template.Version,
                    template.Section,
                    template.Title,
                    template.Checks.Select(static check => new EvaluationCheckResultResponse(
                            check.ControlId,
                            check.CheckId,
                            check.Title,
                            check.Description,
                            check.Method,
                            check.Endpoint,
                            check.GraphPermissions,
                            check.ExpectedResult,
                            check.Status,
                            check.ActualResult,
                            check.RawResult,
                            check.Notes))
                        .ToList()))
                .ToList()
            ?? []);
}

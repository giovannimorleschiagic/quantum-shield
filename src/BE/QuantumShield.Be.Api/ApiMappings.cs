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

    public static EvaluationRunResponse ToResponse(this EvaluationRun run)
        => new(
            run.Id,
            run.TenantId,
            run.Status,
            run.TemplateIdentifier,
            run.TemplateVersion,
            run.TotalChecks,
            run.PassedChecks,
            run.FailedChecks,
            run.NotApplicableChecks,
            run.ErrorMessage,
            run.StartedAtUtc,
            run.CompletedAtUtc,
            run.Results.Select(static result => new EvaluationResultResponse(
                result.Id,
                result.RuleKey,
                result.DisplayName,
                result.Status,
                result.Severity,
                result.ExpectedValue,
                result.ActualValue,
                result.Notes)).ToList());
}

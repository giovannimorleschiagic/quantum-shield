using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Infrastructure.Persistence.Entities;

namespace QuantumShield.Be.Infrastructure.Persistence;

internal static class DomainMapper
{
    public static Tenant ToDomain(this TenantEntity entity)
        => Tenant.Rehydrate(
            entity.Id,
            entity.TenantName,
            entity.TenantId,
            entity.ClientId,
            entity.SecretReference,
            entity.IsActive,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);

    public static TenantEntity ToEntity(this Tenant domain)
        => new()
        {
            Id = domain.Id,
            TenantName = domain.TenantName,
            TenantId = domain.TenantId,
            ClientId = domain.ClientId,
            SecretReference = domain.SecretReference,
            IsActive = domain.IsActive,
            CreatedAtUtc = domain.CreatedAtUtc,
            UpdatedAtUtc = domain.UpdatedAtUtc
        };

    public static EvaluationRun ToDomain(this EvaluationRunEntity entity)
    {
        var run = EvaluationRun.CreatePending(entity.TenantId, entity.TemplateIdentifier, entity.TemplateVersion);
        SetProperty(run, nameof(EvaluationRun.Id), entity.Id);
        SetProperty(run, nameof(EvaluationRun.StartedAtUtc), entity.StartedAtUtc);

        if (entity.Status == Domain.Enums.EvaluationRunStatus.InProgress)
        {
            run.MarkInProgress();
        }

        if (entity.Status == Domain.Enums.EvaluationRunStatus.Completed)
        {
            run.MarkInProgress();
            run.Complete(entity.Results.Select(static result => result.ToDomain()));
            SetProperty(run, nameof(EvaluationRun.CompletedAtUtc), entity.CompletedAtUtc);
        }

        if (entity.Status == Domain.Enums.EvaluationRunStatus.Failed)
        {
            run.MarkInProgress();
            run.Fail(entity.ErrorMessage ?? "The evaluation run failed.");
            SetProperty(run, nameof(EvaluationRun.CompletedAtUtc), entity.CompletedAtUtc);
        }

        return run;
    }

    public static EvaluationRunEntity ToEntity(this EvaluationRun domain)
        => new()
        {
            Id = domain.Id,
            TenantId = domain.TenantId,
            Status = domain.Status,
            TemplateIdentifier = domain.TemplateIdentifier,
            TemplateVersion = domain.TemplateVersion,
            TotalChecks = domain.TotalChecks,
            PassedChecks = domain.PassedChecks,
            FailedChecks = domain.FailedChecks,
            NotApplicableChecks = domain.NotApplicableChecks,
            ErrorMessage = domain.ErrorMessage,
            StartedAtUtc = domain.StartedAtUtc,
            CompletedAtUtc = domain.CompletedAtUtc,
            Results = domain.Results.Select(static result => result.ToEntity()).ToList()
        };

    public static EvaluationResult ToDomain(this EvaluationResultEntity entity)
    {
        var result = new EvaluationResult(
            entity.RuleKey,
            entity.DisplayName,
            entity.Status,
            entity.Severity,
            entity.ExpectedValue,
            entity.ActualValue,
            entity.Notes);

        SetProperty(result, nameof(EvaluationResult.Id), entity.Id);
        return result;
    }

    public static EvaluationResultEntity ToEntity(this EvaluationResult domain)
        => new()
        {
            Id = domain.Id,
            RuleKey = domain.RuleKey,
            DisplayName = domain.DisplayName,
            Status = domain.Status,
            Severity = domain.Severity,
            ExpectedValue = domain.ExpectedValue,
            ActualValue = domain.ActualValue,
            Notes = domain.Notes
        };

    private static void SetProperty<T>(T target, string propertyName, object? value)
    {
        typeof(T).GetProperty(propertyName)!.SetValue(target, value);
    }
}

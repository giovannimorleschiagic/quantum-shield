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
            entity.IsB2C,
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
            IsB2C = domain.IsB2C,
            CreatedAtUtc = domain.CreatedAtUtc,
            UpdatedAtUtc = domain.UpdatedAtUtc
        };

    public static EvaluationRun ToDomain(this EvaluationRunEntity entity)
    {
        var run = EvaluationRun.CreatePending(entity.TenantId);
        SetProperty(run, nameof(EvaluationRun.Id), entity.Id);
        SetProperty(run, nameof(EvaluationRun.StartedAtUtc), entity.StartedAtUtc);

        if (entity.Status == Domain.Enums.EvaluationRunStatus.InProgress)
        {
            run.MarkInProgress();
        }

        if (entity.Status == Domain.Enums.EvaluationRunStatus.Completed)
        {
            run.MarkInProgress();
            run.Complete(entity.ResultBlobName ?? string.Empty);
            SetProperty(run, nameof(EvaluationRun.CompletedAtUtc), entity.CompletedAtUtc);
        }

        if (entity.Status == Domain.Enums.EvaluationRunStatus.Failed)
        {
            run.MarkInProgress();
            run.Fail();
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
            ResultBlobName = domain.ResultBlobName,
            StartedAtUtc = domain.StartedAtUtc,
            CompletedAtUtc = domain.CompletedAtUtc
        };

    private static void SetProperty<T>(T target, string propertyName, object? value)
    {
        typeof(T).GetProperty(propertyName)!.SetValue(target, value);
    }
}

using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationRun
{
    private EvaluationRun()
    {
    }

    private EvaluationRun(Guid tenantId)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Status = EvaluationRunStatus.Pending;
        StartedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public EvaluationRunStatus Status { get; private set; }

    public string? ResultBlobName { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public EvaluationArtifactDocument? Artifact { get; private set; }

    public static EvaluationRun CreatePending(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new DomainValidationException("Tenant id is required.");
        }

        return new EvaluationRun(tenantId);
    }

    public void MarkInProgress()
    {
        if (Status != EvaluationRunStatus.Pending)
        {
            throw new DomainValidationException("Only pending runs can move to in progress.");
        }

        Status = EvaluationRunStatus.InProgress;
    }

    public void Complete(string resultBlobName)
    {
        if (Status != EvaluationRunStatus.InProgress)
        {
            throw new DomainValidationException("Only in-progress runs can be completed.");
        }

        if (string.IsNullOrWhiteSpace(resultBlobName))
        {
            throw new DomainValidationException("Result blob name is required.");
        }

        ResultBlobName = resultBlobName.Trim();
        Status = EvaluationRunStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AttachArtifact(EvaluationArtifactDocument? artifact)
    {
        Artifact = artifact;
    }

    public void Fail()
    {
        if (Status is EvaluationRunStatus.Completed or EvaluationRunStatus.Failed)
        {
            throw new DomainValidationException("Completed or failed runs cannot transition again.");
        }

        Status = EvaluationRunStatus.Failed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }
}

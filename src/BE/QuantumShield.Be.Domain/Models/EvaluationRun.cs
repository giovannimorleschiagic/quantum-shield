using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class EvaluationRun
{
    private readonly List<EvaluationResult> _results = [];

    private EvaluationRun()
    {
    }

    private EvaluationRun(Guid tenantId, string templateIdentifier, string? templateVersion)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        TemplateIdentifier = templateIdentifier;
        TemplateVersion = templateVersion;
        Status = EvaluationRunStatus.Pending;
        StartedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public EvaluationRunStatus Status { get; private set; }

    public string TemplateIdentifier { get; private set; } = string.Empty;

    public string? TemplateVersion { get; private set; }

    public int TotalChecks { get; private set; }

    public int PassedChecks { get; private set; }

    public int FailedChecks { get; private set; }

    public int NotApplicableChecks { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<EvaluationResult> Results => _results.AsReadOnly();

    public static EvaluationRun CreatePending(Guid tenantId, string templateIdentifier, string? templateVersion)
    {
        if (tenantId == Guid.Empty)
        {
            throw new DomainValidationException("Tenant id is required.");
        }

        if (string.IsNullOrWhiteSpace(templateIdentifier))
        {
            throw new DomainValidationException("Template identifier is required.");
        }

        return new EvaluationRun(tenantId, templateIdentifier.Trim(), templateVersion?.Trim());
    }

    public void MarkInProgress()
    {
        if (Status != EvaluationRunStatus.Pending)
        {
            throw new DomainValidationException("Only pending runs can move to in progress.");
        }

        Status = EvaluationRunStatus.InProgress;
    }

    public void AssignTemplateMetadata(string templateIdentifier, string? templateVersion)
    {
        if (string.IsNullOrWhiteSpace(templateIdentifier))
        {
            throw new DomainValidationException("Template identifier is required.");
        }

        TemplateIdentifier = templateIdentifier.Trim();
        TemplateVersion = templateVersion?.Trim();
    }

    public void Complete(IEnumerable<EvaluationResult> results)
    {
        if (Status != EvaluationRunStatus.InProgress)
        {
            throw new DomainValidationException("Only in-progress runs can be completed.");
        }

        _results.Clear();
        _results.AddRange(results);
        TotalChecks = _results.Count;
        PassedChecks = _results.Count(static item => item.Status == EvaluationCheckStatus.Passed);
        FailedChecks = _results.Count(static item => item.Status == EvaluationCheckStatus.Failed);
        NotApplicableChecks = _results.Count(static item => item.Status == EvaluationCheckStatus.NotApplicable);
        Status = EvaluationRunStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void Fail(string errorMessage)
    {
        if (Status is EvaluationRunStatus.Completed or EvaluationRunStatus.Failed)
        {
            throw new DomainValidationException("Completed or failed runs cannot transition again.");
        }

        Status = EvaluationRunStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "The evaluation run failed." : errorMessage.Trim();
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }
}

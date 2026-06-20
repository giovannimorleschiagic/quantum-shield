using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Business.Services;

public sealed class EvaluationRunService : IEvaluationRunService
{
    private readonly IEvaluationRunRepository _evaluationRunRepository;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly ITenantCredentialProvider _tenantCredentialProvider;
    private readonly ITenantDataCollector _tenantDataCollector;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITemplateProvider _templateProvider;

    public EvaluationRunService(
        ITenantRepository tenantRepository,
        IEvaluationRunRepository evaluationRunRepository,
        ITemplateProvider templateProvider,
        ITenantCredentialProvider tenantCredentialProvider,
        ITenantDataCollector tenantDataCollector,
        IPolicyEvaluator policyEvaluator)
    {
        _tenantRepository = tenantRepository;
        _evaluationRunRepository = evaluationRunRepository;
        _templateProvider = templateProvider;
        _tenantCredentialProvider = tenantCredentialProvider;
        _tenantDataCollector = tenantDataCollector;
        _policyEvaluator = policyEvaluator;
    }

    public Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken)
        => _evaluationRunRepository.ListAsync(cancellationToken);

    public Task<EvaluationRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _evaluationRunRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
        => _evaluationRunRepository.ListByTenantAsync(tenantId, cancellationToken);

    public async Task<EvaluationRun> TriggerAsync(Guid tenantId, string? templateIdentifier, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{tenantId}' was not found.");

        var requestedTemplateIdentifier = string.IsNullOrWhiteSpace(templateIdentifier)
            ? "default-template"
            : templateIdentifier.Trim();

        var run = EvaluationRun.CreatePending(tenant.Id, requestedTemplateIdentifier, null);

        await _evaluationRunRepository.AddAsync(run, cancellationToken);

        try
        {
            var template = await _templateProvider.LoadAsync(templateIdentifier, cancellationToken);
            run.AssignTemplateMetadata(template.TemplateIdentifier, template.Version);
            run.MarkInProgress();
            await _evaluationRunRepository.UpdateAsync(run, cancellationToken);

            var clientSecret = await _tenantCredentialProvider.GetClientSecretAsync(tenant, cancellationToken);
            var snapshot = await _tenantDataCollector.CollectAsync(tenant, clientSecret, template, cancellationToken);
            var results = _policyEvaluator.Evaluate(template, snapshot);

            run.Complete(results);
            await _evaluationRunRepository.UpdateAsync(run, cancellationToken);
            return run;
        }
        catch (Exception exception)
        {
            run.Fail(exception.Message);
            await _evaluationRunRepository.UpdateAsync(run, cancellationToken);
            return run;
        }
    }
}

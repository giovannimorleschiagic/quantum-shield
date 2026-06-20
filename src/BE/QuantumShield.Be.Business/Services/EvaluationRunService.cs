using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Business.Services;

public sealed class EvaluationRunService : IEvaluationRunService
{
    private readonly IEvaluationArtifactStore _artifactStore;
    private readonly ICatalogEvaluationRunner _catalogEvaluationRunner;
    private readonly IEvaluationRunRepository _evaluationRunRepository;
    private readonly ITenantCredentialProvider _tenantCredentialProvider;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITemplateCatalogProvider _templateCatalogProvider;

    public EvaluationRunService(
        ITenantRepository tenantRepository,
        IEvaluationRunRepository evaluationRunRepository,
        ITemplateCatalogProvider templateCatalogProvider,
        ITenantCredentialProvider tenantCredentialProvider,
        ICatalogEvaluationRunner catalogEvaluationRunner,
        IEvaluationArtifactStore artifactStore)
    {
        _tenantRepository = tenantRepository;
        _evaluationRunRepository = evaluationRunRepository;
        _templateCatalogProvider = templateCatalogProvider;
        _tenantCredentialProvider = tenantCredentialProvider;
        _catalogEvaluationRunner = catalogEvaluationRunner;
        _artifactStore = artifactStore;
    }

    public Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken)
        => _evaluationRunRepository.ListAsync(cancellationToken);

    public Task<EvaluationRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => GetAndAttachArtifactAsync(id, cancellationToken);

    public Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
        => _evaluationRunRepository.ListByTenantAsync(tenantId, cancellationToken);

    public async Task<EvaluationRun> TriggerAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{tenantId}' was not found.");

        var run = EvaluationRun.CreatePending(tenant.Id);

        await _evaluationRunRepository.AddAsync(run, cancellationToken);

        try
        {
            var templates = await _templateCatalogProvider.LoadCatalogAsync(cancellationToken);
            run.MarkInProgress();
            await _evaluationRunRepository.UpdateAsync(run, cancellationToken);

            var clientSecret = await _tenantCredentialProvider.GetClientSecretAsync(tenant, cancellationToken);
            var artifact = await _catalogEvaluationRunner.RunAsync(tenant, clientSecret, run.Id, run.StartedAtUtc, templates, cancellationToken);
            var blobName = await _artifactStore.SaveAsync(artifact, cancellationToken);

            run.Complete(blobName);
            run.AttachArtifact(artifact);
            await _evaluationRunRepository.UpdateAsync(run, cancellationToken);
            return run;
        }
        catch (Exception exception)
        {
            _ = exception;
            run.Fail();
            await _evaluationRunRepository.UpdateAsync(run, cancellationToken);
            return run;
        }
    }

    private async Task<EvaluationRun?> GetAndAttachArtifactAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await _evaluationRunRepository.GetByIdAsync(id, cancellationToken);
        if (run is null || string.IsNullOrWhiteSpace(run.ResultBlobName))
        {
            return run;
        }

        var artifact = await _artifactStore.GetAsync(run.ResultBlobName, cancellationToken);
        run.AttachArtifact(artifact);
        return run;
    }
}

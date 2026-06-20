using NSubstitute;
using QuantumShield.Be.Business.Services;
using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Tests.Business;

public sealed class EvaluationRunServiceTests
{
    [Fact]
    public async Task TriggerAsync_ShouldCompleteRun_WhenDependenciesSucceed()
    {
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref");
        var tenantRepository = Substitute.For<ITenantRepository>();
        var runRepository = Substitute.For<IEvaluationRunRepository>();
        var templateCatalogProvider = Substitute.For<ITemplateCatalogProvider>();
        var credentialProvider = Substitute.For<ITenantCredentialProvider>();
        var catalogEvaluationRunner = Substitute.For<ICatalogEvaluationRunner>();
        var artifactStore = Substitute.For<IEvaluationArtifactStore>();

        tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        var templates = new[]
        {
            new EvaluationTemplateDefinition(
                "1.1.1",
                "CIS Microsoft 365 Foundations Benchmark",
                "v1",
                "Section",
                "Rule 1",
                [new EvaluationCheckDefinition("C1", "Description", "graph_api", "GET /organization", ["Organization.Read.All"], "tenantType = AAD")])
        };
        var artifact = new EvaluationArtifactDocument(
            Guid.NewGuid(),
            tenant.Id,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            EvaluationRunStatus.Completed,
            new EvaluationArtifactSummary(1, 1, 0, 0, 1, 0),
            [new EvaluationArtifactTemplateResult("1.1.1", "CIS Microsoft 365 Foundations Benchmark", "v1", "Section", "Rule 1", [
                new EvaluationCheckResult("1.1.1", "C1", "Rule 1", "Description", "graph_api", "GET /organization", ["Organization.Read.All"], "tenantType = AAD", EvaluationCheckStatus.Passed, "AAD", "{\"tenantType\":\"AAD\"}", null)
            ])]);

        templateCatalogProvider.LoadCatalogAsync(Arg.Any<CancellationToken>()).Returns(templates);
        credentialProvider.GetClientSecretAsync(tenant, Arg.Any<CancellationToken>()).Returns("secret");
        catalogEvaluationRunner.RunAsync(tenant, "secret", Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<IReadOnlyCollection<EvaluationTemplateDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(artifact);
        artifactStore.SaveAsync(artifact, Arg.Any<CancellationToken>()).Returns("tenant-a/run-a.json");

        var service = new EvaluationRunService(tenantRepository, runRepository, templateCatalogProvider, credentialProvider, catalogEvaluationRunner, artifactStore);

        var run = await service.TriggerAsync(tenant.Id, CancellationToken.None);

        Assert.Equal(EvaluationRunStatus.Completed, run.Status);
        Assert.Equal("tenant-a/run-a.json", run.ResultBlobName);
        await runRepository.Received(1).AddAsync(Arg.Any<EvaluationRun>(), Arg.Any<CancellationToken>());
        await runRepository.Received(2).UpdateAsync(Arg.Any<EvaluationRun>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerAsync_ShouldMarkRunFailed_WhenTemplateLoadFails()
    {
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref");
        var tenantRepository = Substitute.For<ITenantRepository>();
        var runRepository = Substitute.For<IEvaluationRunRepository>();
        var templateCatalogProvider = Substitute.For<ITemplateCatalogProvider>();
        var credentialProvider = Substitute.For<ITenantCredentialProvider>();
        var catalogEvaluationRunner = Substitute.For<ICatalogEvaluationRunner>();
        var artifactStore = Substitute.For<IEvaluationArtifactStore>();

        tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        templateCatalogProvider.LoadCatalogAsync(Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyCollection<EvaluationTemplateDefinition>>>(_ => throw new InvalidOperationException("Catalog missing."));

        var service = new EvaluationRunService(tenantRepository, runRepository, templateCatalogProvider, credentialProvider, catalogEvaluationRunner, artifactStore);

        var run = await service.TriggerAsync(tenant.Id, CancellationToken.None);

        Assert.Equal(EvaluationRunStatus.Failed, run.Status);
        await runRepository.Received(1).AddAsync(Arg.Any<EvaluationRun>(), Arg.Any<CancellationToken>());
        await runRepository.Received(1).UpdateAsync(Arg.Is<EvaluationRun>(item => item.Status == EvaluationRunStatus.Failed), Arg.Any<CancellationToken>());
    }
}

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
        var templateProvider = Substitute.For<ITemplateProvider>();
        var credentialProvider = Substitute.For<ITenantCredentialProvider>();
        var dataCollector = Substitute.For<ITenantDataCollector>();
        var evaluator = Substitute.For<IPolicyEvaluator>();

        tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        templateProvider.LoadAsync("template-a", Arg.Any<CancellationToken>())
            .Returns(new EvaluationTemplateDefinition(
                "template-a",
                "v1",
                [new EvaluationRuleDefinition("rule-1", "Rule 1", "organization.displayName", EvaluationComparisonType.Equals, EvaluationSeverity.Low, "Contoso")]));
        credentialProvider.GetClientSecretAsync(tenant, Arg.Any<CancellationToken>()).Returns("secret");
        dataCollector.CollectAsync(tenant, "secret", Arg.Any<EvaluationTemplateDefinition>(), Arg.Any<CancellationToken>())
            .Returns(new TenantEvaluationSnapshot(new Dictionary<string, string?> { ["organization.displayName"] = "Contoso" }));
        evaluator.Evaluate(Arg.Any<EvaluationTemplateDefinition>(), Arg.Any<TenantEvaluationSnapshot>())
            .Returns([new EvaluationResult("rule-1", "Rule 1", EvaluationCheckStatus.Passed, EvaluationSeverity.Low, "Contoso", "Contoso", null)]);

        var service = new EvaluationRunService(tenantRepository, runRepository, templateProvider, credentialProvider, dataCollector, evaluator);

        var run = await service.TriggerAsync(tenant.Id, "template-a", CancellationToken.None);

        Assert.Equal(EvaluationRunStatus.Completed, run.Status);
        await runRepository.Received(1).AddAsync(Arg.Any<EvaluationRun>(), Arg.Any<CancellationToken>());
        await runRepository.Received(2).UpdateAsync(Arg.Any<EvaluationRun>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerAsync_ShouldMarkRunFailed_WhenTemplateLoadFails()
    {
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref");
        var tenantRepository = Substitute.For<ITenantRepository>();
        var runRepository = Substitute.For<IEvaluationRunRepository>();
        var templateProvider = Substitute.For<ITemplateProvider>();
        var credentialProvider = Substitute.For<ITenantCredentialProvider>();
        var dataCollector = Substitute.For<ITenantDataCollector>();
        var evaluator = Substitute.For<IPolicyEvaluator>();

        tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        templateProvider.LoadAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task<EvaluationTemplateDefinition>>(_ => throw new InvalidOperationException("Blob missing."));

        var service = new EvaluationRunService(tenantRepository, runRepository, templateProvider, credentialProvider, dataCollector, evaluator);

        var run = await service.TriggerAsync(tenant.Id, null, CancellationToken.None);

        Assert.Equal(EvaluationRunStatus.Failed, run.Status);
        Assert.Equal("Blob missing.", run.ErrorMessage);
        await runRepository.Received(1).AddAsync(Arg.Any<EvaluationRun>(), Arg.Any<CancellationToken>());
        await runRepository.Received(1).UpdateAsync(Arg.Is<EvaluationRun>(item => item.Status == EvaluationRunStatus.Failed), Arg.Any<CancellationToken>());
    }
}

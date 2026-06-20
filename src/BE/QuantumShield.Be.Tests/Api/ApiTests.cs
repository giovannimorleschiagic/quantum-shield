using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using QuantumShield.Be.Api.Contracts;
using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Tests.Api;

public sealed class ApiTests
{
    [Fact]
    public async Task TenantsEndpoint_ShouldCreateAndReturnTenant()
    {
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest(
            "Contoso",
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "plain-secret",
            true));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdTenant = await response.Content.ReadFromJsonAsync<TenantResponse>();
        Assert.NotNull(createdTenant);
        Assert.StartsWith("https://vault.example/secrets/", createdTenant.SecretReference, StringComparison.Ordinal);
        Assert.DoesNotContain("plain-secret", createdTenant.SecretReference, StringComparison.Ordinal);

        var list = await client.GetFromJsonAsync<List<TenantResponse>>("/api/tenants");

        Assert.NotNull(list);
        Assert.Contains(list, item => item.Id == createdTenant.Id);
    }

    [Fact]
    public async Task TriggerRunEndpoint_ShouldReturnAcceptedRun()
    {
        await using var factory = new TestApplicationFactory();
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref");
        factory.TenantRepository.Seed(tenant);

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/evaluations/runs", new TriggerEvaluationRunRequest(tenant.Id, "template-a"));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var run = await response.Content.ReadFromJsonAsync<EvaluationRunResponse>();
        Assert.NotNull(run);
        Assert.Equal(EvaluationRunStatus.Completed, run.Status);
        Assert.Equal(1, run.TotalChecks);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        public InMemoryTenantRepository TenantRepository { get; } = new();

        private InMemoryRunRepository RunRepository { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddLogging(logging => logging.ClearProviders());
                services.RemoveAll<ITenantRepository>();
                services.RemoveAll<IEvaluationRunRepository>();
                services.RemoveAll<ITemplateProvider>();
                services.RemoveAll<ITenantCredentialProvider>();
                services.RemoveAll<ITenantDataCollector>();
                services.RemoveAll<IPolicyEvaluator>();

                services.AddSingleton<ITenantRepository>(TenantRepository);
                services.AddSingleton<IEvaluationRunRepository>(RunRepository);
                services.AddSingleton<ITemplateProvider>(new FakeTemplateProvider());
                services.AddSingleton<ITenantCredentialProvider>(new FakeCredentialProvider());
                services.AddSingleton<ITenantDataCollector>(new FakeDataCollector());
                services.AddSingleton<IPolicyEvaluator>(new FakePolicyEvaluator());
            });
        }
    }

    public sealed class InMemoryTenantRepository : ITenantRepository
    {
        private readonly Dictionary<Guid, Tenant> _store = [];

        public void Seed(Tenant tenant) => _store[tenant.Id] = tenant;

        public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_store.GetValueOrDefault(id));

        public Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<Tenant>>(_store.Values.OrderBy(static item => item.TenantName).ToList());

        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken)
        {
            _store[tenant.Id] = tenant;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken)
        {
            _store[tenant.Id] = tenant;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryRunRepository : IEvaluationRunRepository
    {
        private readonly Dictionary<Guid, EvaluationRun> _store = [];

        public Task<EvaluationRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_store.GetValueOrDefault(id));

        public Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<EvaluationRun>>(_store.Values.OrderByDescending(static item => item.StartedAtUtc).ToList());

        public Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<EvaluationRun>>(_store.Values.Where(item => item.TenantId == tenantId).ToList());

        public Task AddAsync(EvaluationRun run, CancellationToken cancellationToken)
        {
            _store[run.Id] = run;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(EvaluationRun run, CancellationToken cancellationToken)
        {
            _store[run.Id] = run;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTemplateProvider : ITemplateProvider
    {
        public Task<EvaluationTemplateDefinition> LoadAsync(string? templateIdentifier, CancellationToken cancellationToken)
            => Task.FromResult(new EvaluationTemplateDefinition(
                templateIdentifier ?? "default-template",
                "v1",
                [new EvaluationRuleDefinition("rule-1", "Rule 1", "organization.displayName", EvaluationComparisonType.Equals, EvaluationSeverity.Low, "Contoso")]));
    }

    private sealed class FakeCredentialProvider : ITenantCredentialProvider
    {
        public Task<string> SaveClientSecretAsync(Guid tenantId, string clientSecret, CancellationToken cancellationToken)
            => Task.FromResult($"https://vault.example/secrets/{tenantId:N}-client-secret");

        public Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken cancellationToken)
            => Task.FromResult("secret");
    }

    private sealed class FakeDataCollector : ITenantDataCollector
    {
        public Task<TenantEvaluationSnapshot> CollectAsync(Tenant tenant, string clientSecret, EvaluationTemplateDefinition template, CancellationToken cancellationToken)
            => Task.FromResult(new TenantEvaluationSnapshot(new Dictionary<string, string?> { ["organization.displayName"] = "Contoso" }));
    }

    private sealed class FakePolicyEvaluator : IPolicyEvaluator
    {
        public IReadOnlyCollection<EvaluationResult> Evaluate(EvaluationTemplateDefinition template, TenantEvaluationSnapshot snapshot)
            => [new EvaluationResult("rule-1", "Rule 1", EvaluationCheckStatus.Passed, EvaluationSeverity.Low, "Contoso", "Contoso", null)];
    }
}

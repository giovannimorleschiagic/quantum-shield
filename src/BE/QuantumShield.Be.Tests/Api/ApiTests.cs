using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.SchemeName);

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
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        var response = await client.PostAsJsonAsync("/api/evaluations/runs", new TriggerEvaluationRunRequest(tenant.Id));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var run = await response.Content.ReadFromJsonAsync<EvaluationRunSummaryResponse>();
        Assert.NotNull(run);
        Assert.Equal(EvaluationRunStatus.Completed, run.Status);
        Assert.Equal("tenant-a/evaluation-a.json", run.ResultBlobName);

        var detail = await client.GetFromJsonAsync<EvaluationRunDetailResponse>($"/api/evaluations/runs/{run.Id}");
        Assert.NotNull(detail);
        Assert.NotNull(detail.Summary);
        Assert.Single(detail.Templates);
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
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, static _ => { });
                services.RemoveAll<ITenantRepository>();
                services.RemoveAll<IEvaluationRunRepository>();
                services.RemoveAll<ITemplateCatalogProvider>();
                services.RemoveAll<ITenantCredentialProvider>();
                services.RemoveAll<ICatalogEvaluationRunner>();
                services.RemoveAll<IEvaluationArtifactStore>();

                services.AddSingleton<ITenantRepository>(TenantRepository);
                services.AddSingleton<IEvaluationRunRepository>(RunRepository);
                services.AddSingleton<ITemplateCatalogProvider>(new FakeTemplateCatalogProvider());
                services.AddSingleton<ITenantCredentialProvider>(new FakeCredentialProvider());
                services.AddSingleton<ICatalogEvaluationRunner>(new FakeCatalogEvaluationRunner());
                services.AddSingleton<IEvaluationArtifactStore>(new FakeArtifactStore());
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

    private sealed class FakeTemplateCatalogProvider : ITemplateCatalogProvider
    {
        public Task<IReadOnlyCollection<EvaluationTemplateDefinition>> LoadCatalogAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<EvaluationTemplateDefinition>>([
                new EvaluationTemplateDefinition(
                "1.1.1",
                "CIS Microsoft 365 Foundations Benchmark",
                "v1",
                "Section",
                "Rule 1",
                [new EvaluationCheckDefinition("C1", "Description", "graph_api", "GET /organization", ["Organization.Read.All"], "tenantType = AAD")])
            ]);
    }

    private sealed class FakeCredentialProvider : ITenantCredentialProvider
    {
        public Task<string> SaveClientSecretAsync(Guid tenantId, string clientSecret, CancellationToken cancellationToken)
            => Task.FromResult($"https://vault.example/secrets/{tenantId:N}-client-secret");

        public Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken cancellationToken)
            => Task.FromResult("secret");
    }

    private sealed class FakeCatalogEvaluationRunner : ICatalogEvaluationRunner
    {
        public Task<EvaluationArtifactDocument> RunAsync(Tenant tenant, string clientSecret, Guid evaluationId, DateTimeOffset startedAtUtc, IReadOnlyCollection<EvaluationTemplateDefinition> templates, CancellationToken cancellationToken)
            => Task.FromResult(new EvaluationArtifactDocument(
                evaluationId,
                tenant.Id,
                startedAtUtc,
                startedAtUtc.AddMinutes(1),
                EvaluationRunStatus.Completed,
                new EvaluationArtifactSummary(1, 1, 0, 0, 1, 0),
                [new EvaluationArtifactTemplateResult("1.1.1", "CIS Microsoft 365 Foundations Benchmark", "v1", "Section", "Rule 1", [
                    new EvaluationCheckResult("1.1.1", "C1", "Rule 1", "Description", "graph_api", "GET /organization", ["Organization.Read.All"], "tenantType = AAD", EvaluationCheckStatus.Passed, "AAD", "{\"tenantType\":\"AAD\"}", null)
                ])]));
    }

    private sealed class FakeArtifactStore : IEvaluationArtifactStore
    {
        private readonly Dictionary<string, EvaluationArtifactDocument> _artifacts = [];

        public Task<string> SaveAsync(EvaluationArtifactDocument artifact, CancellationToken cancellationToken)
        {
            const string blobName = "tenant-a/evaluation-a.json";
            _artifacts[blobName] = artifact;
            return Task.FromResult(blobName);
        }

        public Task<EvaluationArtifactDocument?> GetAsync(string blobName, CancellationToken cancellationToken)
            => Task.FromResult(_artifacts.TryGetValue(blobName, out var artifact) ? artifact : null);
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim("tid", "7f8ac17c-e18d-4f8e-a8ec-9fb46868bb8f"),
                new Claim("aud", "8e50ea44-eb28-45bf-84d4-752025d25b46")
            ], SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

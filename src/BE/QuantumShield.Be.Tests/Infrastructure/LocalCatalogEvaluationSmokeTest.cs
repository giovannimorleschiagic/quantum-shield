using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuantumShield.Be.Domain.Options;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Infrastructure.Evaluation;
using QuantumShield.Be.Infrastructure.Templates;

namespace QuantumShield.Be.Tests.Infrastructure;

public sealed class LocalCatalogEvaluationSmokeTest
{
    // [Fact]
    public async Task RunCatalogEvaluationFromLocalTemplates_ShouldWriteArtifactToLocalFile()
    {
        var settings = LocalExecutionSettings.CreateConfigured();
        var hostEnvironment = new FixedHostEnvironment(settings.ContentRootPath);
        var catalogProvider = new FileSystemTemplateCatalogProvider(hostEnvironment);
        var runner = new CatalogEvaluationRunner(Options.Create(new GraphOptions()));

        var templates = await catalogProvider.LoadCatalogAsync(CancellationToken.None);
        var tenant = Tenant.Create(
            settings.TenantName,
            settings.DirectoryTenantId,
            settings.ClientId,
            "local-secret-reference",
            isB2C: true);
        var evaluationId = Guid.NewGuid();
        var startedAtUtc = DateTimeOffset.UtcNow;

        var artifact = await runner.RunAsync(
            tenant,
            settings.ClientSecret,
            evaluationId,
            startedAtUtc,
            templates,
            CancellationToken.None);

        Directory.CreateDirectory(Path.GetDirectoryName(settings.OutputFilePath)!);
        await File.WriteAllTextAsync(
            settings.OutputFilePath,
            JsonSerializer.Serialize(artifact, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            }),
            CancellationToken.None);

        Assert.True(File.Exists(settings.OutputFilePath));
    }

    private sealed class FixedHostEnvironment : IHostEnvironment
    {
        public FixedHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ApplicationName = "QuantumShield.Be.Tests";
            EnvironmentName = Environments.Development;
            ContentRootFileProvider = null!;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; }

        public string ContentRootPath { get; set; }

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
    }

    private sealed record LocalExecutionSettings(
        string TenantName,
        string DirectoryTenantId,
        string ClientId,
        string ClientSecret,
        string OutputFilePath,
        string ContentRootPath)
    {
        public static LocalExecutionSettings CreateConfigured()
        {
            const string tenantName = "";
            const string directoryTenantId = "";
            const string clientId = "";
            const string clientSecret = "";

            var values = new Dictionary<string, string>
            {
                ["TenantName"] = tenantName,
                ["DirectoryTenantId"] = directoryTenantId,
                ["ClientId"] = clientId,
                ["ClientSecret"] = clientSecret
            };

            var missing = values.Where(static item => item.Value.StartsWith("PASTE_", StringComparison.Ordinal) || item.Value.StartsWith("LOCAL_", StringComparison.Ordinal))
                .Select(static item => item.Key)
                .ToList();

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Configure the local smoke test credentials in {nameof(LocalExecutionSettings)} before running it. Missing: {string.Join(", ", missing)}.");
            }

            var outputFilePath = Path.Combine(
                Path.GetTempPath(),
                "quantum-shield",
                "local-evaluation-results",
                $"evaluation-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");

            var contentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "QuantumShield.Be.Api"));

            return new LocalExecutionSettings(
                tenantName,
                directoryTenantId,
                clientId,
                clientSecret,
                outputFilePath,
                contentRootPath);
        }
    }
}

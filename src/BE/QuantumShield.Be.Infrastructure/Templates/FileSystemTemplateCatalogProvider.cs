using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Templates;

public sealed class FileSystemTemplateCatalogProvider : ITemplateCatalogProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IHostEnvironment _hostEnvironment;

    public FileSystemTemplateCatalogProvider(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<IReadOnlyCollection<EvaluationTemplateDefinition>> LoadCatalogAsync(CancellationToken cancellationToken)
    {
        var templateDirectory = ResolveTemplateDirectory();
        var templates = new List<EvaluationTemplateDefinition>();

        foreach (var filePath in Directory.EnumerateFiles(templateDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            await using var stream = File.OpenRead(filePath);
            var document = await JsonSerializer.DeserializeAsync<TemplateDocument>(stream, SerializerOptions, cancellationToken)
                ?? throw new InvalidOperationException($"Template '{filePath}' is malformed.");

            templates.Add(new EvaluationTemplateDefinition(
                document.ControlId,
                document.Benchmark,
                document.Version,
                document.Section,
                document.Title,
                document.Checks.Select(static check => new EvaluationCheckDefinition(
                        check.CheckId,
                        check.Description,
                        check.Method,
                        check.Endpoint,
                        check.GraphPermissions ?? [],
                        check.ExpectedResult,
                        check.IsSupportedForB2C ?? true))
                    .ToList()));
        }

        return templates;
    }

    private string ResolveTemplateDirectory()
    {
        var outputCandidate = Path.Combine(AppContext.BaseDirectory, "templates", "cis-m365-v310");
        if (Directory.Exists(outputCandidate))
        {
            return outputCandidate;
        }

        var current = new DirectoryInfo(_hostEnvironment.ContentRootPath);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "templates", "cis-m365-v310");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("The local template catalog directory 'templates\\cis-m365-v310' could not be found.");
    }

    private sealed record TemplateDocument(
        string Benchmark,
        string Version,
        [property: JsonPropertyName("control_id")] string ControlId,
        string Section,
        string Title,
        List<TemplateCheckDocument> Checks);

    private sealed record TemplateCheckDocument(
        [property: JsonPropertyName("check_id")] string CheckId,
        string Description,
        string Method,
        string? Endpoint,
        [property: JsonPropertyName("expected_result")] string ExpectedResult,
        [property: JsonPropertyName("graph_permissions")] List<string>? GraphPermissions,
        [property: JsonPropertyName("b2c_supported")] bool? IsSupportedForB2C);
}

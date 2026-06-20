using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using QuantumShield.Be.Domain.Enums;
using QuantumShield.Be.Domain.Exceptions;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Domain.Options;

namespace QuantumShield.Be.Infrastructure.Templates;

public sealed class BlobTemplateProvider : ITemplateProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly BlobContainerClient _blobContainerClient;
    private readonly BlobStorageOptions _options;

    public BlobTemplateProvider(BlobContainerClient blobContainerClient, Microsoft.Extensions.Options.IOptions<BlobStorageOptions> options)
    {
        _blobContainerClient = blobContainerClient;
        _options = options.Value;
    }

    public async Task<EvaluationTemplateDefinition> LoadAsync(string? templateIdentifier, CancellationToken cancellationToken)
    {
        var blobName = string.IsNullOrWhiteSpace(templateIdentifier)
            ? _options.DefaultTemplateBlobName
            : templateIdentifier.Trim();

        try
        {
            var response = await _blobContainerClient.GetBlobClient(blobName).DownloadContentAsync(cancellationToken);
            var templateDocument = response.Value.Content.ToObjectFromJson<BlobTemplateDocument>(SerializerOptions)
                ?? throw new DomainValidationException("Template content could not be parsed.");

            var rules = templateDocument.Rules.Select(static rule => new EvaluationRuleDefinition(
                    rule.Key,
                    rule.DisplayName,
                    rule.DataPath,
                    Enum.Parse<EvaluationComparisonType>(rule.ComparisonType, ignoreCase: true),
                    Enum.Parse<EvaluationSeverity>(rule.Severity, ignoreCase: true),
                    rule.ExpectedValue))
                .ToList();

            return new EvaluationTemplateDefinition(templateDocument.TemplateIdentifier, templateDocument.Version, rules);
        }
        catch (RequestFailedException exception)
        {
            throw new DomainValidationException($"The evaluation template '{blobName}' could not be loaded. {exception.Message}");
        }
        catch (JsonException exception)
        {
            throw new DomainValidationException($"The evaluation template '{blobName}' is malformed. {exception.Message}");
        }
    }

    internal sealed record BlobTemplateDocument(string TemplateIdentifier, string? Version, List<BlobTemplateRuleDocument> Rules);

    internal sealed record BlobTemplateRuleDocument(
        string Key,
        string DisplayName,
        string DataPath,
        string ComparisonType,
        string Severity,
        string? ExpectedValue);
}

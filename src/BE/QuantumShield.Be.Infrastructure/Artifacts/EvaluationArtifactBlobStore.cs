using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Artifacts;

public sealed class EvaluationArtifactBlobStore : IEvaluationArtifactStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly BlobContainerClient _blobContainerClient;

    public EvaluationArtifactBlobStore(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<string> SaveAsync(EvaluationArtifactDocument artifact, CancellationToken cancellationToken)
    {
        await _blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = $"{artifact.TenantId:D}/{artifact.EvaluationId:D}.json";
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        using var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(artifact, SerializerOptions));
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken);
        return blobName;
    }

    public async Task<EvaluationArtifactDocument?> GetAsync(string blobName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return null;
        }

        try
        {
            var response = await _blobContainerClient.GetBlobClient(blobName.Trim()).DownloadContentAsync(cancellationToken);
            return response.Value.Content.ToObjectFromJson<EvaluationArtifactDocument>(SerializerOptions);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }
}

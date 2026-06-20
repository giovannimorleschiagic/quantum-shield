using System.ComponentModel.DataAnnotations;

namespace QuantumShield.Be.Domain.Options;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    [Required]
    public string ContainerName { get; init; } = string.Empty;

    [Required]
    public string DefaultTemplateBlobName { get; init; } = string.Empty;
}

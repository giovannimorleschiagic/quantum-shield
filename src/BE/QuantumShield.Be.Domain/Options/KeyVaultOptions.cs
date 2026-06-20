using System.ComponentModel.DataAnnotations;

namespace QuantumShield.Be.Domain.Options;

public sealed class KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    [Required]
    public string VaultUri { get; init; } = string.Empty;
}

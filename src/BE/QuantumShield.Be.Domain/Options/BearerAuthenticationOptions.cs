using System.ComponentModel.DataAnnotations;

namespace QuantumShield.Be.Domain.Options;

public sealed class BearerAuthenticationOptions
{
    public const string SectionName = "Authentication";

    [Required]
    public string TenantId { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace QuantumShield.Be.Domain.Options;

public sealed class GraphOptions
{
    public const string SectionName = "Graph";

    [Required]
    public string[] Scopes { get; init; } = ["https://graph.microsoft.com/.default"];
}

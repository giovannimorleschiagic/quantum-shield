using System.ComponentModel.DataAnnotations;

namespace QuantumShield.Be.Domain.Options;

public sealed class SqlDatabaseOptions
{
    public const string SectionName = "SqlDatabase";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}

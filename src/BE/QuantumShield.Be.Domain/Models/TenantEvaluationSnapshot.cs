namespace QuantumShield.Be.Domain.Models;

public sealed class TenantEvaluationSnapshot
{
    public TenantEvaluationSnapshot(IReadOnlyDictionary<string, string?> values)
    {
        Values = values;
    }

    public IReadOnlyDictionary<string, string?> Values { get; }
}

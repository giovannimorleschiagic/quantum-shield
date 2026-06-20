using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface ITenantDataCollector
{
    Task<TenantEvaluationSnapshot> CollectAsync(
        Tenant tenant,
        string clientSecret,
        EvaluationTemplateDefinition template,
        CancellationToken cancellationToken);
}

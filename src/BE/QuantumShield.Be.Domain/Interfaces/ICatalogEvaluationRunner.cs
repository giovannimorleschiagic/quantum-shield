using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface ICatalogEvaluationRunner
{
    Task<EvaluationArtifactDocument> RunAsync(
        Tenant tenant,
        string clientSecret,
        Guid evaluationId,
        DateTimeOffset startedAtUtc,
        IReadOnlyCollection<EvaluationTemplateDefinition> templates,
        CancellationToken cancellationToken);
}

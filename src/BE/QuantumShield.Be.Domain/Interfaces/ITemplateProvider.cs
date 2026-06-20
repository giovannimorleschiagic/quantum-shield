using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface ITemplateProvider
{
    Task<EvaluationTemplateDefinition> LoadAsync(string? templateIdentifier, CancellationToken cancellationToken);
}

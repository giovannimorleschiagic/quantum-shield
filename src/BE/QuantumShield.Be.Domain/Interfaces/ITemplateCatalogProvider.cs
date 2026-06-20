using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface ITemplateCatalogProvider
{
    Task<IReadOnlyCollection<EvaluationTemplateDefinition>> LoadCatalogAsync(CancellationToken cancellationToken);
}

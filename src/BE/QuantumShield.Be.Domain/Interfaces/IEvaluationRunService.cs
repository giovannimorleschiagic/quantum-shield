using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface IEvaluationRunService
{
    Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken);

    Task<EvaluationRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<EvaluationRun> TriggerAsync(Guid tenantId, CancellationToken cancellationToken);
}

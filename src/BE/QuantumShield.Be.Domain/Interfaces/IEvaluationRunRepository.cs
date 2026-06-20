using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface IEvaluationRunRepository
{
    Task<EvaluationRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken);

    Task AddAsync(EvaluationRun run, CancellationToken cancellationToken);

    Task UpdateAsync(EvaluationRun run, CancellationToken cancellationToken);
}

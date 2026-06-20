using Microsoft.EntityFrameworkCore;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Persistence.Repositories;

public sealed class EvaluationRunRepository : IEvaluationRunRepository
{
    private readonly ZeroTrustDbContext _dbContext;

    public EvaluationRunRepository(ZeroTrustDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EvaluationRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.EvaluationRuns.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.EvaluationRuns.AsNoTracking()
            .OrderByDescending(item => item.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(static item => item.ToDomain()).ToList();
    }

    public async Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.EvaluationRuns.AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(static item => item.ToDomain()).ToList();
    }

    public async Task AddAsync(EvaluationRun run, CancellationToken cancellationToken)
    {
        await _dbContext.EvaluationRuns.AddAsync(run.ToEntity(), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EvaluationRun run, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.EvaluationRuns
            .SingleAsync(item => item.Id == run.Id, cancellationToken);

        existing.Status = run.Status;
        existing.ResultBlobName = run.ResultBlobName;
        existing.StartedAtUtc = run.StartedAtUtc;
        existing.CompletedAtUtc = run.CompletedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

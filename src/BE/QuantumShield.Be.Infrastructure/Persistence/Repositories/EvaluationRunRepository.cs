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
            .Include(item => item.Results)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyCollection<EvaluationRun>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.EvaluationRuns.AsNoTracking()
            .Include(item => item.Results)
            .OrderByDescending(item => item.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(static item => item.ToDomain()).ToList();
    }

    public async Task<IReadOnlyCollection<EvaluationRun>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.EvaluationRuns.AsNoTracking()
            .Include(item => item.Results)
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
            .Include(item => item.Results)
            .SingleAsync(item => item.Id == run.Id, cancellationToken);

        _dbContext.EvaluationResults.RemoveRange(existing.Results);

        existing.Status = run.Status;
        existing.TemplateIdentifier = run.TemplateIdentifier;
        existing.TemplateVersion = run.TemplateVersion;
        existing.TotalChecks = run.TotalChecks;
        existing.PassedChecks = run.PassedChecks;
        existing.FailedChecks = run.FailedChecks;
        existing.NotApplicableChecks = run.NotApplicableChecks;
        existing.ErrorMessage = run.ErrorMessage;
        existing.StartedAtUtc = run.StartedAtUtc;
        existing.CompletedAtUtc = run.CompletedAtUtc;
        existing.Results = run.Results.Select(static result => result.ToEntity()).ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

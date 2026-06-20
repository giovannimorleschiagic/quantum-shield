using Microsoft.EntityFrameworkCore;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly ZeroTrustDbContext _dbContext;

    public TenantRepository(ZeroTrustDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Tenants.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Tenants.AsNoTracking()
            .OrderBy(item => item.TenantName)
            .ToListAsync(cancellationToken);

        return entities.Select(static item => item.ToDomain()).ToList();
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        await _dbContext.Tenants.AddAsync(tenant.ToEntity(), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        _dbContext.Tenants.Update(tenant.ToEntity());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

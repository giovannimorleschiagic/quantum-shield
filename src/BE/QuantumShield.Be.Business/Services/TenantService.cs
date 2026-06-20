using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Business.Services;

public sealed class TenantService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken cancellationToken)
        => _tenantRepository.ListAsync(cancellationToken);

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _tenantRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Tenant> CreateAsync(
        string tenantName,
        string tenantId,
        string clientId,
        string secretReference,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(tenantName, tenantId, clientId, secretReference, isActive);
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        return tenant;
    }

    public async Task<Tenant?> UpdateAsync(
        Guid id,
        string tenantName,
        string tenantId,
        string clientId,
        string secretReference,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        tenant.Update(tenantName, tenantId, clientId, secretReference, isActive);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        return tenant;
    }
}

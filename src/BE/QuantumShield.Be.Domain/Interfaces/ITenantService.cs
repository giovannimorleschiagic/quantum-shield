using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface ITenantService
{
    Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken cancellationToken);

    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Tenant> CreateAsync(
        string tenantName,
        string tenantId,
        string clientId,
        string clientSecret,
        bool isActive,
        bool isB2C,
        CancellationToken cancellationToken);

    Task<Tenant?> UpdateAsync(
        Guid id,
        string tenantName,
        string tenantId,
        string clientId,
        string clientSecret,
        bool isActive,
        bool isB2C,
        CancellationToken cancellationToken);
}

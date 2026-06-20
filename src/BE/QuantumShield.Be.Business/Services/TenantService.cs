using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Business.Services;

public sealed class TenantService : ITenantService
{
    private readonly ITenantCredentialProvider _tenantCredentialProvider;
    private readonly ITenantRepository _tenantRepository;

    public TenantService(ITenantRepository tenantRepository, ITenantCredentialProvider tenantCredentialProvider)
    {
        _tenantRepository = tenantRepository;
        _tenantCredentialProvider = tenantCredentialProvider;
    }

    public Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken cancellationToken)
        => _tenantRepository.ListAsync(cancellationToken);

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _tenantRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Tenant> CreateAsync(
        string tenantName,
        string tenantId,
        string clientId,
        string clientSecret,
        bool isActive,
        CancellationToken cancellationToken)
    {
        ValidateClientSecret(clientSecret);

        var tenantIdValue = Guid.NewGuid();
        var secretReference = await _tenantCredentialProvider.SaveClientSecretAsync(tenantIdValue, clientSecret, cancellationToken);
        var tenant = Tenant.Create(tenantIdValue, tenantName, tenantId, clientId, secretReference, isActive);
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        return tenant;
    }

    public async Task<Tenant?> UpdateAsync(
        Guid id,
        string tenantName,
        string tenantId,
        string clientId,
        string clientSecret,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        ValidateClientSecret(clientSecret);

        var secretReference = await _tenantCredentialProvider.SaveClientSecretAsync(tenant.Id, clientSecret, cancellationToken);
        tenant.Update(tenantName, tenantId, clientId, secretReference, isActive);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        return tenant;
    }

    private static void ValidateClientSecret(string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new DomainValidationException("Client secret is required.");
        }
    }
}

using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface ITenantCredentialProvider
{
    Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken cancellationToken);
}

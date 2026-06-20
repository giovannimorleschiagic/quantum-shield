using Azure.Security.KeyVault.Secrets;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Security;

public sealed class KeyVaultTenantCredentialProvider : ITenantCredentialProvider
{
    private readonly SecretClient _secretClient;

    public KeyVaultTenantCredentialProvider(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }

    public async Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var secretName = ExtractSecretName(tenant.SecretReference);
        var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
        return secret.Value.Value;
    }

    public static string ExtractSecretName(string secretReference)
    {
        if (Uri.TryCreate(secretReference, UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2 && string.Equals(segments[0], "secrets", StringComparison.OrdinalIgnoreCase))
            {
                return segments[1];
            }
        }

        return secretReference;
    }
}

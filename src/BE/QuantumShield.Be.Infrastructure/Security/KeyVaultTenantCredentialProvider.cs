using Azure.Security.KeyVault.Secrets;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Infrastructure.Security;

public sealed class KeyVaultTenantCredentialProvider : ITenantCredentialProvider
{
    private const string SecretNameSuffix = "client-secret";
    private readonly SecretClient _secretClient;

    public KeyVaultTenantCredentialProvider(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }

    public async Task<string> SaveClientSecretAsync(Guid tenantId, string clientSecret, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("Client secret is required.");
        }

        var secretName = BuildSecretName(tenantId);
        var secret = await _secretClient.SetSecretAsync(secretName, clientSecret.Trim(), cancellationToken);
        return secret.Value.Id?.ToString() ?? secret.Value.Name;
    }

    public async Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var secretName = ExtractSecretName(tenant.SecretReference);
        var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
        return secret.Value.Value;
    }

    public static string BuildSecretName(Guid tenantId)
        => $"tenant-{tenantId:N}-{SecretNameSuffix}";

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

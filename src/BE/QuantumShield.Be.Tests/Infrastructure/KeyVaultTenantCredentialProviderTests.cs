using QuantumShield.Be.Infrastructure.Security;

namespace QuantumShield.Be.Tests.Infrastructure;

public sealed class KeyVaultTenantCredentialProviderTests
{
    [Fact]
    public void ExtractSecretName_ShouldHandleFullSecretUri()
    {
        var result = KeyVaultTenantCredentialProvider.ExtractSecretName("https://vault-name.vault.azure.net/secrets/client-secret/version");

        Assert.Equal("client-secret", result);
    }
}

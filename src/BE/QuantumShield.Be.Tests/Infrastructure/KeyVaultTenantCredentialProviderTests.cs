using QuantumShield.Be.Infrastructure.Security;

namespace QuantumShield.Be.Tests.Infrastructure;

public sealed class KeyVaultTenantCredentialProviderTests
{
    [Fact]
    public void BuildSecretName_ShouldUseStableTenantPattern()
    {
        var tenantId = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var result = KeyVaultTenantCredentialProvider.BuildSecretName(tenantId);

        Assert.Equal("tenant-11111111222233334444555555555555-client-secret", result);
    }

    [Fact]
    public void ExtractSecretName_ShouldHandleFullSecretUri()
    {
        var result = KeyVaultTenantCredentialProvider.ExtractSecretName("https://vault-name.vault.azure.net/secrets/client-secret/version");

        Assert.Equal("client-secret", result);
    }
}

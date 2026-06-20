using NSubstitute;
using QuantumShield.Be.Business.Services;
using QuantumShield.Be.Domain.Exceptions;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Tests.Business;

public sealed class TenantServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistKeyVaultReference_WhenSecretIsProvided()
    {
        var tenantRepository = Substitute.For<ITenantRepository>();
        var credentialProvider = Substitute.For<ITenantCredentialProvider>();
        credentialProvider
            .SaveClientSecretAsync(Arg.Any<Guid>(), "plain-secret", Arg.Any<CancellationToken>())
            .Returns("https://vault.example/secrets/tenant-secret");

        var service = new TenantService(tenantRepository, credentialProvider);

        var tenant = await service.CreateAsync(
            "Contoso",
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "plain-secret",
            true,
            CancellationToken.None);

        Assert.Equal("https://vault.example/secrets/tenant-secret", tenant.SecretReference);
        await credentialProvider.Received(1).SaveClientSecretAsync(tenant.Id, "plain-secret", Arg.Any<CancellationToken>());
        await tenantRepository.Received(1).AddAsync(Arg.Is<Tenant>(item => item.SecretReference == "https://vault.example/secrets/tenant-secret"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectEmptySecret()
    {
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref");
        var tenantRepository = Substitute.For<ITenantRepository>();
        var credentialProvider = Substitute.For<ITenantCredentialProvider>();
        tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);

        var service = new TenantService(tenantRepository, credentialProvider);

        var action = () => service.UpdateAsync(
            tenant.Id,
            tenant.TenantName,
            tenant.TenantId,
            tenant.ClientId,
            "",
            tenant.IsActive,
            CancellationToken.None);

        await Assert.ThrowsAsync<DomainValidationException>(action);
        await credentialProvider.DidNotReceiveWithAnyArgs().SaveClientSecretAsync(default, default!, default);
    }
}

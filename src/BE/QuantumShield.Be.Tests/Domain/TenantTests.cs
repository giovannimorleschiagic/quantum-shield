using QuantumShield.Be.Domain.Exceptions;
using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Tests.Domain;

public sealed class TenantTests
{
    [Fact]
    public void Create_ShouldRejectInvalidTenantId()
    {
        var action = () => Tenant.Create("Contoso", "not-a-guid", Guid.NewGuid().ToString(), "secret-ref");

        var exception = Assert.Throws<DomainValidationException>(action);
        Assert.Equal("Tenant id must be a valid GUID.", exception.Message);
    }

    [Fact]
    public void Create_ShouldPersistB2CFlag()
    {
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref", true, true);

        Assert.True(tenant.IsB2C);
    }
}

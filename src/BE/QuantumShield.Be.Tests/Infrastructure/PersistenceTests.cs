using Microsoft.EntityFrameworkCore;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Infrastructure.Persistence;
using QuantumShield.Be.Infrastructure.Persistence.Repositories;

namespace QuantumShield.Be.Tests.Infrastructure;

public sealed class PersistenceTests
{
    [Fact]
    public async Task TenantRepository_ShouldPersistTenant()
    {
        await using var dbContext = CreateDbContext();
        var repository = new TenantRepository(dbContext);
        var tenant = Tenant.Create("Contoso", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret-ref");

        await repository.AddAsync(tenant, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(tenant.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal("Contoso", loaded.TenantName);
    }

    private static ZeroTrustDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ZeroTrustDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ZeroTrustDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuantumShield.Be.Infrastructure.Persistence;

public sealed class ZeroTrustDbContextFactory : IDesignTimeDbContextFactory<ZeroTrustDbContext>
{
    public ZeroTrustDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ZeroTrustDbContext>();
        builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=QuantumShield.Be;Trusted_Connection=True;TrustServerCertificate=True");
        return new ZeroTrustDbContext(builder.Options);
    }
}

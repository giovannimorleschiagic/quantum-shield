using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumShield.Be.Infrastructure.Persistence.Migrations
{
    public partial class AddTenantB2CFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Tenants', 'IsB2C') IS NULL
                BEGIN
                    ALTER TABLE [Tenants] ADD [IsB2C] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Tenants', 'IsB2C') IS NOT NULL
                BEGIN
                    ALTER TABLE [Tenants] DROP COLUMN [IsB2C];
                END
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumShield.Be.Infrastructure.Persistence.Migrations
{
    public partial class AddTenantB2CFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsB2C",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsB2C",
                table: "Tenants");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumShield.Be.Infrastructure.Persistence.Migrations
{
    public partial class CatalogEvaluationRunRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationResults");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "EvaluationRuns");

            migrationBuilder.DropColumn(
                name: "FailedChecks",
                table: "EvaluationRuns");

            migrationBuilder.DropColumn(
                name: "NotApplicableChecks",
                table: "EvaluationRuns");

            migrationBuilder.DropColumn(
                name: "PassedChecks",
                table: "EvaluationRuns");

            migrationBuilder.DropColumn(
                name: "TemplateIdentifier",
                table: "EvaluationRuns");

            migrationBuilder.DropColumn(
                name: "TemplateVersion",
                table: "EvaluationRuns");

            migrationBuilder.DropColumn(
                name: "TotalChecks",
                table: "EvaluationRuns");

            migrationBuilder.AddColumn<string>(
                name: "ResultBlobName",
                table: "EvaluationRuns",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultBlobName",
                table: "EvaluationRuns");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "EvaluationRuns",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedChecks",
                table: "EvaluationRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NotApplicableChecks",
                table: "EvaluationRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PassedChecks",
                table: "EvaluationRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TemplateIdentifier",
                table: "EvaluationRuns",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "TemplateVersion",
                table: "EvaluationRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalChecks",
                table: "EvaluationRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EvaluationResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExpectedValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RuleKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationResults_EvaluationRuns_EvaluationRunId",
                        column: x => x.EvaluationRunId,
                        principalTable: "EvaluationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationResults_EvaluationRunId",
                table: "EvaluationResults",
                column: "EvaluationRunId");
        }
    }
}

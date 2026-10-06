using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurakshaSetu.Migrations
{
    /// <inheritdoc />
    public partial class AddSafetyReportingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "SafetyReports",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ImmediateDanger",
                table: "SafetyReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MedicalAssistanceRequired",
                table: "SafetyReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                table: "SafetyReports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "SafetyReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SecurityAssistanceRequired",
                table: "SafetyReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "SafetyReports",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "SafetyReports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SafetyReportStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SafetyReportId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyReportStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyReportStatusHistories_SafetyReports_SafetyReportId",
                        column: x => x.SafetyReportId,
                        principalTable: "SafetyReports",
                        principalColumn: "SafetyReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SafetyReportStatusHistories_SafetyReportId",
                table: "SafetyReportStatusHistories",
                column: "SafetyReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SafetyReportStatusHistories");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "ImmediateDanger",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "MedicalAssistanceRequired",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "SecurityAssistanceRequired",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "SafetyReports");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "SafetyReports");
        }
    }
}

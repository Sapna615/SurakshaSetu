using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurakshaSetu.Migrations
{
    /// <inheritdoc />
    public partial class AddSafetyReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SafetyReports",
                columns: table => new
                {
                    SafetyReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncidentCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IncidentLocation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncidentTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyReports", x => x.SafetyReportId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SafetyReports");
        }
    }
}

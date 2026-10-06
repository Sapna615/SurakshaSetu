using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurakshaSetu.Migrations
{
    /// <inheritdoc />
    public partial class AddSafetyMapModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationType",
                table: "CampusLocations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SafetyAlerts",
                columns: table => new
                {
                    SafetyAlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyAlerts", x => x.SafetyAlertId);
                    table.ForeignKey(
                        name: "FK_SafetyAlerts_CampusLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CampusLocations",
                        principalColumn: "CampusLocationId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SafetyAlerts_LocationId",
                table: "SafetyAlerts",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "CampusLocations");
        }
    }
}

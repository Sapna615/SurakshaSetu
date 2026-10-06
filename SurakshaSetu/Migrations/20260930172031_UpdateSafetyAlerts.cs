using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurakshaSetu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSafetyAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafetyAlerts_CampusLocations_LocationId",
                table: "SafetyAlerts");

            migrationBuilder.DropIndex(
                name: "IX_SafetyAlerts_LocationId",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "SafetyAlerts");

            migrationBuilder.RenameColumn(
                name: "ValidUntil",
                table: "SafetyAlerts",
                newName: "PublishedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SafetyAlerts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "SafetyAlerts",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SafetyAlerts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "SafetyAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "SafetyAlerts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "SafetyAlerts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "SafetyAlerts",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedAction",
                table: "SafetyAlerts",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "SafetyAlerts");

            migrationBuilder.DropColumn(
                name: "RecommendedAction",
                table: "SafetyAlerts");

            migrationBuilder.RenameColumn(
                name: "PublishedAt",
                table: "SafetyAlerts",
                newName: "ValidUntil");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SafetyAlerts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "SafetyAlerts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SafetyAlerts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "SafetyAlerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                table: "SafetyAlerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SafetyAlerts_LocationId",
                table: "SafetyAlerts",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyAlerts_CampusLocations_LocationId",
                table: "SafetyAlerts",
                column: "LocationId",
                principalTable: "CampusLocations",
                principalColumn: "CampusLocationId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurakshaSetu.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "IncidentEvidences",
                columns: table => new
                {
                    IncidentEvidenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentEvidences", x => x.IncidentEvidenceId);
                    table.ForeignKey(
                        name: "FK_IncidentEvidences_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEvidences_IncidentId",
                table: "IncidentEvidences",
                column: "IncidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentEvidences");


        }
    }
}

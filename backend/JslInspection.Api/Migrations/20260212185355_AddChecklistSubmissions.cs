using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JslInspection.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistTemplateId = table.Column<int>(type: "int", nullable: false),
                    SubmittedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OverallRemark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistSubmissions_ChecklistTemplates_ChecklistTemplateId",
                        column: x => x.ChecklistTemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistSubmissionItemId = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionPhotos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistSubmissionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistSubmissionId = table.Column<int>(type: "int", nullable: false),
                    ChecklistItemTemplateId = table.Column<int>(type: "int", nullable: false),
                    ValueText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValueNumber = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsNotOk = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistSubmissionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistSubmissionItems_ChecklistSubmissions_ChecklistSubmissionId",
                        column: x => x.ChecklistSubmissionId,
                        principalTable: "ChecklistSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistSubmissionItems_ChecklistSubmissionId_ChecklistItemTemplateId",
                table: "ChecklistSubmissionItems",
                columns: new[] { "ChecklistSubmissionId", "ChecklistItemTemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistSubmissions_ChecklistTemplateId",
                table: "ChecklistSubmissions",
                column: "ChecklistTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionPhotos_ChecklistSubmissionItemId",
                table: "SubmissionPhotos",
                column: "ChecklistSubmissionItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistSubmissionItems");

            migrationBuilder.DropTable(
                name: "SubmissionPhotos");

            migrationBuilder.DropTable(
                name: "ChecklistSubmissions");
        }
    }
}

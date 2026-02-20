using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JslInspection.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistTemplateId = table.Column<int>(type: "int", nullable: false),
                    SeqNo = table.Column<int>(type: "int", nullable: false),
                    ItemText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    InputType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPhotoIfNotOk = table.Column<bool>(type: "bit", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    DropdownOptionsCsv = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItemTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItemTemplates_ChecklistTemplates_ChecklistTemplateId",
                        column: x => x.ChecklistTemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ChecklistTemplates",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Daily visual & basic parameter checks for CRM area.", true, "CRM Daily Inspection" },
                    { 2, new DateTimeOffset(new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Weekly detailed checklist for conveyor health.", true, "CRM Conveyor Weekly Check" }
                });

            migrationBuilder.InsertData(
                table: "ChecklistItemTemplates",
                columns: new[] { "Id", "ChecklistTemplateId", "DropdownOptionsCsv", "InputType", "IsRequired", "ItemText", "MaxValue", "MinValue", "RequiresPhotoIfNotOk", "SeqNo" },
                values: new object[,]
                {
                    { 101, 1, null, "PassFail", true, "Any oil leakage observed?", null, null, true, 1 },
                    { 102, 1, "Good,Average,Poor", "Dropdown", true, "Housekeeping condition", null, null, false, 2 },
                    { 103, 1, null, "Number", true, "Motor temperature (°C)", 90m, 0m, false, 3 },
                    { 104, 1, null, "Text", false, "Remarks (if any)", null, null, false, 4 },
                    { 201, 2, null, "PassFail", true, "Belt alignment OK?", null, null, true, 1 },
                    { 202, 2, null, "PassFail", true, "Abnormal noise/vibration?", null, null, true, 2 },
                    { 203, 2, null, "PassFail", true, "Any damaged idlers/rollers visible?", null, null, true, 3 },
                    { 204, 2, null, "Text", false, "General comments", null, null, false, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItemTemplates_ChecklistTemplateId_SeqNo",
                table: "ChecklistItemTemplates",
                columns: new[] { "ChecklistTemplateId", "SeqNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItemTemplates");

            migrationBuilder.DropTable(
                name: "ChecklistTemplates");
        }
    }
}

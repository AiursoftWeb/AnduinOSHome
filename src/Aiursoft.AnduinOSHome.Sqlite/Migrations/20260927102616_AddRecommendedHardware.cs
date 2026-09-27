using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.AnduinOSHome.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendedHardware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hardware",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Architecture = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Configuration = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PriceUsd = table.Column<int>(type: "INTEGER", nullable: true),
                    PriceMarket = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PriceCheckedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProductUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ReportUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ProductImagePath = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ExperienceImagePath = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ImageSourceUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ImageCredit = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TestedVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    KernelVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DriverVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TeamDevice = table.Column<bool>(type: "INTEGER", nullable: false),
                    Installation = table.Column<int>(type: "INTEGER", nullable: false),
                    Virtualization = table.Column<int>(type: "INTEGER", nullable: false),
                    Performance = table.Column<int>(type: "INTEGER", nullable: false),
                    Wifi = table.Column<int>(type: "INTEGER", nullable: false),
                    Display = table.Column<int>(type: "INTEGER", nullable: false),
                    SecureBoot = table.Column<int>(type: "INTEGER", nullable: false),
                    Graphics = table.Column<int>(type: "INTEGER", nullable: false),
                    Publication = table.Column<int>(type: "INTEGER", nullable: false),
                    Featured = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hardware", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HardwareTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HardwareId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    FirmwareNotes = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    InstallationNotes = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    KnownIssues = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardwareTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HardwareTranslations_Hardware_HardwareId",
                        column: x => x.HardwareId,
                        principalTable: "Hardware",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hardware_Slug",
                table: "Hardware",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HardwareTranslations_HardwareId_Culture",
                table: "HardwareTranslations",
                columns: new[] { "HardwareId", "Culture" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HardwareTranslations");

            migrationBuilder.DropTable(
                name: "Hardware");
        }
    }
}

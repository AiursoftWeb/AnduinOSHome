using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.AnduinOSHome.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareCapabilityDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GraphicsDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InstallationDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PerformanceDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecureBootDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VirtualizationDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WifiDetail",
                table: "HardwareTranslations",
                type: "text",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayDetail",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "GraphicsDetail",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "InstallationDetail",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "PerformanceDetail",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "SecureBootDetail",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "VirtualizationDetail",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "WifiDetail",
                table: "HardwareTranslations");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.AnduinOSHome.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareSourceWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BasedOnSourceRevision",
                table: "HardwareTranslations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceCulture",
                table: "Hardware",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "en")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SourceRevision",
                table: "Hardware",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasedOnSourceRevision",
                table: "HardwareTranslations");

            migrationBuilder.DropColumn(
                name: "SourceCulture",
                table: "Hardware");

            migrationBuilder.DropColumn(
                name: "SourceRevision",
                table: "Hardware");
        }
    }
}

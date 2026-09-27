using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.AnduinOSHome.Sqlite.Migrations
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
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceCulture",
                table: "Hardware",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<int>(
                name: "SourceRevision",
                table: "Hardware",
                type: "INTEGER",
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

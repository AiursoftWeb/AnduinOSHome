using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.AnduinOSHome.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfflineUpdates",
                columns: table => new
                {
                    BatchId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ReleaseLine = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Suite = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Architecture = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    TargetVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TargetPatch = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SigningFingerprint = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Withdrawn = table.Column<bool>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineUpdates", x => x.BatchId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfflineUpdates_ReleaseLine_Suite_Architecture",
                table: "OfflineUpdates",
                columns: new[] { "ReleaseLine", "Suite", "Architecture" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfflineUpdates");
        }
    }
}

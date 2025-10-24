using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulletinBoard.Hosts.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advertisement_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvertisementId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advertisement_files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_advertisement_files_AdvertisementId",
                table: "advertisement_files",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_advertisement_files_AdvertisementId_Order",
                table: "advertisement_files",
                columns: new[] { "AdvertisementId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_advertisement_files_FileId",
                table: "advertisement_files",
                column: "FileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advertisement_files");

            migrationBuilder.DropTable(
                name: "files");
        }
    }
}

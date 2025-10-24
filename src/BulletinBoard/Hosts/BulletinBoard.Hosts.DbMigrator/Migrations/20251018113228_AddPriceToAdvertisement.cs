using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulletinBoard.Hosts.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToAdvertisement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "advertisements",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_CreatedAt",
                table: "advertisements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_Price",
                table: "advertisements",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_Status",
                table: "advertisements",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_advertisements_CreatedAt",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_Price",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_Status",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "advertisements");
        }
    }
}

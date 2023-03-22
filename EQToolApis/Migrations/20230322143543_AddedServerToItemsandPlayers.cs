using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerToItemsandPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Server",
                table: "EQitems",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Server",
                table: "EQAuctionPlayers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_EQitems_ItemName_Server",
                table: "EQitems",
                columns: new[] { "ItemName", "Server" });

            migrationBuilder.CreateIndex(
                name: "IX_EQitems_Server",
                table: "EQitems",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQAuctionPlayers_Server",
                table: "EQAuctionPlayers",
                column: "Server");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQitems_ItemName_Server",
                table: "EQitems");

            migrationBuilder.DropIndex(
                name: "IX_EQitems_Server",
                table: "EQitems");

            migrationBuilder.DropIndex(
                name: "IX_EQAuctionPlayers_Server",
                table: "EQAuctionPlayers");

            migrationBuilder.DropColumn(
                name: "Server",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "Server",
                table: "EQAuctionPlayers");
        }
    }
}

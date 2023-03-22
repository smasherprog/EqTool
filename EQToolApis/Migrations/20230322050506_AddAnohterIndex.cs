using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddAnohterIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_Server_AuctionType",
                table: "EQTunnelMessages",
                columns: new[] { "Server", "AuctionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQTunnelMessages_Server_AuctionType",
                table: "EQTunnelMessages");
        }
    }
}

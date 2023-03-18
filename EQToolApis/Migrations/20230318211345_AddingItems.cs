using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddingItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "EQAuctionPlayers",
                columns: table => new
                {
                    EQAuctionPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EQAuctionPlayers", x => x.EQAuctionPlayerId);
                });

            _ = migrationBuilder.CreateTable(
                name: "EQitems",
                columns: table => new
                {
                    EQitemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EQitems", x => x.EQitemId);
                });

            _ = migrationBuilder.CreateTable(
                name: "EQTunnelMessages",
                columns: table => new
                {
                    EQTunnelMessageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordMessageId = table.Column<long>(type: "bigint", nullable: false),
                    EQAuctionPlayerId = table.Column<int>(type: "int", nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false),
                    AuctionType = table.Column<byte>(type: "tinyint", nullable: false),
                    TunnelTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EQTunnelMessages", x => x.EQTunnelMessageId);
                    _ = table.ForeignKey(
                        name: "FK_EQTunnelMessages_EQAuctionPlayers_EQAuctionPlayerId",
                        column: x => x.EQAuctionPlayerId,
                        principalTable: "EQAuctionPlayers",
                        principalColumn: "EQAuctionPlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "EQTunnelAuctionItems",
                columns: table => new
                {
                    EQTunnelAuctionItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQitemId = table.Column<int>(type: "int", nullable: false),
                    AuctionPrice = table.Column<int>(type: "int", nullable: true),
                    EQTunnelMessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EQTunnelAuctionItems", x => x.EQTunnelAuctionItemId);
                    _ = table.ForeignKey(
                        name: "FK_EQTunnelAuctionItems_EQTunnelMessages_EQTunnelMessageId",
                        column: x => x.EQTunnelMessageId,
                        principalTable: "EQTunnelMessages",
                        principalColumn: "EQTunnelMessageId",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_EQTunnelAuctionItems_EQitems_EQitemId",
                        column: x => x.EQitemId,
                        principalTable: "EQitems",
                        principalColumn: "EQitemId",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_EQTunnelAuctionItems_EQitemId",
                table: "EQTunnelAuctionItems",
                column: "EQitemId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_EQTunnelAuctionItems_EQTunnelMessageId",
                table: "EQTunnelAuctionItems",
                column: "EQTunnelMessageId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_DiscordMessageId",
                table: "EQTunnelMessages",
                column: "DiscordMessageId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_EQAuctionPlayerId",
                table: "EQTunnelMessages",
                column: "EQAuctionPlayerId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_Server",
                table: "EQTunnelMessages",
                column: "Server");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "EqToolExceptions");

            _ = migrationBuilder.DropTable(
                name: "EQTunnelAuctionItems");

            _ = migrationBuilder.DropTable(
                name: "Players");

            _ = migrationBuilder.DropTable(
                name: "EQTunnelMessages");

            _ = migrationBuilder.DropTable(
                name: "EQitems");

            _ = migrationBuilder.DropTable(
                name: "EQAuctionPlayers");
        }
    }
}

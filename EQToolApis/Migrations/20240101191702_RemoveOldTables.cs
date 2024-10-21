using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EQTunnelAuctionItems");

            migrationBuilder.DropTable(
                name: "EQTunnelMessages");

            migrationBuilder.DropTable(
                name: "EQitems");

            migrationBuilder.DropTable(
                name: "EQAuctionPlayers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EQAuctionPlayers",
                columns: table => new
                {
                    EQAuctionPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQAuctionPlayers", x => x.EQAuctionPlayerId);
                });

            migrationBuilder.CreateTable(
                name: "EQitems",
                columns: table => new
                {
                    EQitemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastWTBSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastWTSSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Server = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalWTBAuctionAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBAuctionCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast30DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast30DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast60DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast60DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast6MonthsAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast6MonthsCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast90DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast90DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLastYearAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLastYearCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSAuctionAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSAuctionCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast30DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast30DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast60DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast60DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast6MonthsAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast6MonthsCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast90DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast90DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLastYearAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLastYearCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQitems", x => x.EQitemId);
                });

            migrationBuilder.CreateTable(
                name: "EQTunnelMessages",
                columns: table => new
                {
                    EQTunnelMessageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQAuctionPlayerId = table.Column<int>(type: "int", nullable: false),
                    AuctionType = table.Column<byte>(type: "tinyint", nullable: false),
                    DiscordMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false),
                    TunnelTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQTunnelMessages", x => x.EQTunnelMessageId);
                    table.ForeignKey(
                        name: "FK_EQTunnelMessages_EQAuctionPlayers_EQAuctionPlayerId",
                        column: x => x.EQAuctionPlayerId,
                        principalTable: "EQAuctionPlayers",
                        principalColumn: "EQAuctionPlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EQTunnelAuctionItems",
                columns: table => new
                {
                    EQTunnelAuctionItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQitemId = table.Column<int>(type: "int", nullable: false),
                    EQTunnelMessageId = table.Column<long>(type: "bigint", nullable: false),
                    AuctionPrice = table.Column<int>(type: "int", nullable: true),
                    Server = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQTunnelAuctionItems", x => x.EQTunnelAuctionItemId);
                    table.ForeignKey(
                        name: "FK_EQTunnelAuctionItems_EQTunnelMessages_EQTunnelMessageId",
                        column: x => x.EQTunnelMessageId,
                        principalTable: "EQTunnelMessages",
                        principalColumn: "EQTunnelMessageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EQTunnelAuctionItems_EQitems_EQitemId",
                        column: x => x.EQitemId,
                        principalTable: "EQitems",
                        principalColumn: "EQitemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EQAuctionPlayers_Server",
                table: "EQAuctionPlayers",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQitems_ItemName_Server",
                table: "EQitems",
                columns: new[] { "ItemName", "Server" });

            migrationBuilder.CreateIndex(
                name: "IX_EQitems_Server",
                table: "EQitems",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelAuctionItems_EQitemId",
                table: "EQTunnelAuctionItems",
                column: "EQitemId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelAuctionItems_EQTunnelMessageId",
                table: "EQTunnelAuctionItems",
                column: "EQTunnelMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_AuctionType",
                table: "EQTunnelMessages",
                column: "AuctionType");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_DiscordMessageId",
                table: "EQTunnelMessages",
                column: "DiscordMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_EQAuctionPlayerId",
                table: "EQTunnelMessages",
                column: "EQAuctionPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_Server",
                table: "EQTunnelMessages",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_Server_AuctionType",
                table: "EQTunnelMessages",
                columns: new[] { "Server", "AuctionType" });

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_TunnelTimestamp",
                table: "EQTunnelMessages",
                column: "TunnelTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_TunnelTimestamp_AuctionType",
                table: "EQTunnelMessages",
                columns: new[] { "TunnelTimestamp", "AuctionType" });
        }
    }
}

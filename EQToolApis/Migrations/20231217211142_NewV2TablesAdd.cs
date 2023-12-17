using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class NewV2TablesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EQAuctionPlayersV2",
                columns: table => new
                {
                    EQAuctionPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQAuctionPlayersV2", x => x.EQAuctionPlayerId);
                });

            migrationBuilder.CreateTable(
                name: "EQitemsV2",
                columns: table => new
                {
                    EQitemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false),
                    LastWTBSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastWTSSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalWTSAuctionCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSAuctionAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast30DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast30DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast60DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast60DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast90DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast90DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast6MonthsCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLast6MonthsAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLastYearCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTSLastYearAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBAuctionCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBAuctionAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast30DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast30DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast60DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast60DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast90DaysCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast90DaysAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast6MonthsCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLast6MonthsAverage = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLastYearCount = table.Column<int>(type: "int", nullable: false),
                    TotalWTBLastYearAverage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQitemsV2", x => x.EQitemId);
                });

            migrationBuilder.CreateTable(
                name: "EQTunnelMessagesV2",
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
                    table.PrimaryKey("PK_EQTunnelMessagesV2", x => x.EQTunnelMessageId);
                    table.ForeignKey(
                        name: "FK_EQTunnelMessagesV2_EQAuctionPlayersV2_EQAuctionPlayerId",
                        column: x => x.EQAuctionPlayerId,
                        principalTable: "EQAuctionPlayersV2",
                        principalColumn: "EQAuctionPlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EQTunnelAuctionItemsV2",
                columns: table => new
                {
                    EQTunnelAuctionItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQitemId = table.Column<int>(type: "int", nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false),
                    AuctionPrice = table.Column<int>(type: "int", nullable: true),
                    EQTunnelMessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQTunnelAuctionItemsV2", x => x.EQTunnelAuctionItemId);
                    table.ForeignKey(
                        name: "FK_EQTunnelAuctionItemsV2_EQTunnelMessagesV2_EQTunnelMessageId",
                        column: x => x.EQTunnelMessageId,
                        principalTable: "EQTunnelMessagesV2",
                        principalColumn: "EQTunnelMessageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EQTunnelAuctionItemsV2_EQitemsV2_EQitemId",
                        column: x => x.EQitemId,
                        principalTable: "EQitemsV2",
                        principalColumn: "EQitemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EQAuctionPlayersV2_Server",
                table: "EQAuctionPlayersV2",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQitemsV2_ItemName_Server",
                table: "EQitemsV2",
                columns: new[] { "ItemName", "Server" });

            migrationBuilder.CreateIndex(
                name: "IX_EQitemsV2_Server",
                table: "EQitemsV2",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelAuctionItemsV2_EQitemId",
                table: "EQTunnelAuctionItemsV2",
                column: "EQitemId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelAuctionItemsV2_EQTunnelMessageId",
                table: "EQTunnelAuctionItemsV2",
                column: "EQTunnelMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_AuctionType",
                table: "EQTunnelMessagesV2",
                column: "AuctionType");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_DiscordMessageId",
                table: "EQTunnelMessagesV2",
                column: "DiscordMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_EQAuctionPlayerId",
                table: "EQTunnelMessagesV2",
                column: "EQAuctionPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_Server",
                table: "EQTunnelMessagesV2",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_Server_AuctionType",
                table: "EQTunnelMessagesV2",
                columns: new[] { "Server", "AuctionType" });

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_TunnelTimestamp",
                table: "EQTunnelMessagesV2",
                column: "TunnelTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessagesV2_TunnelTimestamp_AuctionType",
                table: "EQTunnelMessagesV2",
                columns: new[] { "TunnelTimestamp", "AuctionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EQTunnelAuctionItemsV2");

            migrationBuilder.DropTable(
                name: "EQTunnelMessagesV2");

            migrationBuilder.DropTable(
                name: "EQitemsV2");

            migrationBuilder.DropTable(
                name: "EQAuctionPlayersV2");
        }
    }
}

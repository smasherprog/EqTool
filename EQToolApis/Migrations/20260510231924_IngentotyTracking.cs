using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class IngentotyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharacterInventories",
                columns: table => new
                {
                    CharacterInventoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordUserId = table.Column<int>(type: "int", nullable: false),
                    CharacterName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterInventories", x => x.CharacterInventoryId);
                    table.ForeignKey(
                        name: "FK_CharacterInventories_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "DiscordUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterInventoryItems",
                columns: table => new
                {
                    CharacterInventoryItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterInventoryId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Slots = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterInventoryItems", x => x.CharacterInventoryItemId);
                    table.ForeignKey(
                        name: "FK_CharacterInventoryItems_CharacterInventories_CharacterInventoryId",
                        column: x => x.CharacterInventoryId,
                        principalTable: "CharacterInventories",
                        principalColumn: "CharacterInventoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterInventories_DiscordUserId",
                table: "CharacterInventories",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterInventoryItems_CharacterInventoryId",
                table: "CharacterInventoryItems",
                column: "CharacterInventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterInventoryItems");

            migrationBuilder.DropTable(
                name: "CharacterInventories");
        }
    }
}

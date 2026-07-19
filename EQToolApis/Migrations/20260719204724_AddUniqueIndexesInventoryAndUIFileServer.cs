using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexesInventoryAndUIFileServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UIFiles_DiscordUserId_FileName",
                table: "UIFiles");

            migrationBuilder.DropIndex(
                name: "IX_CharacterInventories_DiscordUserId",
                table: "CharacterInventories");

            migrationBuilder.CreateIndex(
                name: "IX_UIFiles_DiscordUserId_FileName_Server",
                table: "UIFiles",
                columns: new[] { "DiscordUserId", "FileName", "Server" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterInventories_DiscordUserId_CharacterName_Server",
                table: "CharacterInventories",
                columns: new[] { "DiscordUserId", "CharacterName", "Server" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UIFiles_DiscordUserId_FileName_Server",
                table: "UIFiles");

            migrationBuilder.DropIndex(
                name: "IX_CharacterInventories_DiscordUserId_CharacterName_Server",
                table: "CharacterInventories");

            migrationBuilder.CreateIndex(
                name: "IX_UIFiles_DiscordUserId_FileName",
                table: "UIFiles",
                columns: new[] { "DiscordUserId", "FileName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterInventories_DiscordUserId",
                table: "CharacterInventories",
                column: "DiscordUserId");
        }
    }
}

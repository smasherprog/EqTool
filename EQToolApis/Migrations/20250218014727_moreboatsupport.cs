using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class moreboatsupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQBoatActivites_Server_Zone_LastSeen",
                table: "EQBoatActivites");

            migrationBuilder.AddColumn<int>(
                name: "Boat",
                table: "EQBoatActivites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EQBoatActivites_Server",
                table: "EQBoatActivites",
                column: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_EQBoatActivites_Server_Boat_Zone_LastSeen",
                table: "EQBoatActivites",
                columns: new[] { "Server", "Boat", "Zone", "LastSeen" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQBoatActivites_Server",
                table: "EQBoatActivites");

            migrationBuilder.DropIndex(
                name: "IX_EQBoatActivites_Server_Boat_Zone_LastSeen",
                table: "EQBoatActivites");

            migrationBuilder.DropColumn(
                name: "Boat",
                table: "EQBoatActivites");

            migrationBuilder.CreateIndex(
                name: "IX_EQBoatActivites_Server_Zone_LastSeen",
                table: "EQBoatActivites",
                columns: new[] { "Server", "Zone", "LastSeen" });
        }
    }
}

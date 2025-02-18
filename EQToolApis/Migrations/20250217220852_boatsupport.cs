using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class boatsupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EQBoatActivites",
                columns: table => new
                {
                    EQBoatActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Zone = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Server = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQBoatActivites", x => x.EQBoatActivityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EQBoatActivites_Server_Zone_LastSeen",
                table: "EQBoatActivites",
                columns: new[] { "Server", "Zone", "LastSeen" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EQBoatActivites");
        }
    }
}

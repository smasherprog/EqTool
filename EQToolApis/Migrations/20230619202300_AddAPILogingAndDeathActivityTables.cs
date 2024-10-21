using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddAPILogingAndDeathActivityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APILogs",
                columns: table => new
                {
                    APILogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    APIAction = table.Column<short>(type: "smallint", nullable: false),
                    LogMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APILogs", x => x.APILogId);
                });

            migrationBuilder.CreateTable(
                name: "EQZones",
                columns: table => new
                {
                    EQZoneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQZones", x => x.EQZoneId);
                });

            migrationBuilder.CreateTable(
                name: "IPBans",
                columns: table => new
                {
                    IpAddress = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPBans", x => x.IpAddress);
                });

            migrationBuilder.CreateTable(
                name: "EQDeaths",
                columns: table => new
                {
                    EQDeathId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQZoneId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    LocX = table.Column<double>(type: "float", nullable: true),
                    LocY = table.Column<double>(type: "float", nullable: true),
                    EQDeathTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQDeaths", x => x.EQDeathId);
                    table.ForeignKey(
                        name: "FK_EQDeaths_EQZones_EQZoneId",
                        column: x => x.EQZoneId,
                        principalTable: "EQZones",
                        principalColumn: "EQZoneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EQDeaths_EQZoneId",
                table: "EQDeaths",
                column: "EQZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APILogs");

            migrationBuilder.DropTable(
                name: "EQDeaths");

            migrationBuilder.DropTable(
                name: "IPBans");

            migrationBuilder.DropTable(
                name: "EQZones");
        }
    }
}

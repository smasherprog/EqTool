using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddNPCTrackingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "APILogs",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(24)",
                oldMaxLength: 24,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EQNotableNPCs",
                columns: table => new
                {
                    EQNotableNPCId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQZoneId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQNotableNPCs", x => x.EQNotableNPCId);
                    table.ForeignKey(
                        name: "FK_EQNotableNPCs_EQZones_EQZoneId",
                        column: x => x.EQZoneId,
                        principalTable: "EQZones",
                        principalColumn: "EQZoneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EQNotableActivities",
                columns: table => new
                {
                    EQNotableActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Server = table.Column<byte>(type: "tinyint", nullable: false),
                    LocX = table.Column<double>(type: "float", nullable: true),
                    LocY = table.Column<double>(type: "float", nullable: true),
                    IsDeath = table.Column<bool>(type: "bit", nullable: false),
                    ActivityTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EQNotableNPCId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQNotableActivities", x => x.EQNotableActivityId);
                    table.ForeignKey(
                        name: "FK_EQNotableActivities_EQNotableNPCs_EQNotableNPCId",
                        column: x => x.EQNotableNPCId,
                        principalTable: "EQNotableNPCs",
                        principalColumn: "EQNotableNPCId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EQNotableActivities_EQNotableNPCId",
                table: "EQNotableActivities",
                column: "EQNotableNPCId");

            migrationBuilder.CreateIndex(
                name: "IX_EQNotableNPCs_EQZoneId",
                table: "EQNotableNPCs",
                column: "EQZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EQNotableActivities");

            migrationBuilder.DropTable(
                name: "EQNotableNPCs");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "APILogs",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(24)",
                oldMaxLength: 24);
        }
    }
}

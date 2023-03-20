using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddedWTSbLastSeen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastSeen",
                table: "EQitems",
                newName: "LastWTSSeen");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastWTBSeen",
                table: "EQitems",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWTBSeen",
                table: "EQitems");

            migrationBuilder.RenameColumn(
                name: "LastWTSSeen",
                table: "EQitems",
                newName: "LastSeen");
        }
    }
}

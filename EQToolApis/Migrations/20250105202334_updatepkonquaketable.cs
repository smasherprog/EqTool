using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class updatepkonquaketable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuakeTimes",
                table: "QuakeTimes");

            migrationBuilder.AddColumn<int>(
                name: "QuakeTimeId",
                table: "QuakeTimes",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuakeTimes",
                table: "QuakeTimes",
                column: "QuakeTimeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuakeTimes",
                table: "QuakeTimes");

            migrationBuilder.DropColumn(
                name: "QuakeTimeId",
                table: "QuakeTimes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuakeTimes",
                table: "QuakeTimes",
                column: "DateTime");
        }
    }
}

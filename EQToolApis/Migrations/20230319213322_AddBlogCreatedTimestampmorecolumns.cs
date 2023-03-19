using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogCreatedTimestampmorecolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalAuctionCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast30DaysCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast60DaysCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast6MonthsCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast90DaysCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLastYearCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAuctionCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast30DaysCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast60DaysCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast6MonthsCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast90DaysCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLastYearCount",
                table: "EQitems");
        }
    }
}

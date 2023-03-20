using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddedWTBcolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalWTBAuctionAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBAuctionCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast30DaysAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast30DaysCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast60DaysAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast60DaysCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast6MonthsAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast6MonthsCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast90DaysAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLast90DaysCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLastYearAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWTBLastYearCount",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_TunnelTimestamp_AuctionType",
                table: "EQTunnelMessages",
                columns: new[] { "TunnelTimestamp", "AuctionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQTunnelMessages_TunnelTimestamp_AuctionType",
                table: "EQTunnelMessages");

            migrationBuilder.DropColumn(
                name: "TotalWTBAuctionAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBAuctionCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast30DaysAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast30DaysCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast60DaysAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast60DaysCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast6MonthsAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast6MonthsCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast90DaysAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLast90DaysCount",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLastYearAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalWTBLastYearCount",
                table: "EQitems");
        }
    }
}

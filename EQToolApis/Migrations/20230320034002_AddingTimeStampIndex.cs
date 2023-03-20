using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddingTimeStampIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalAuctionAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast30DaysAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast60DaysAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast6MonthsAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLast90DaysAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLastYearAverage",
                table: "EQitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EQTunnelMessages_TunnelTimestamp",
                table: "EQTunnelMessages",
                column: "TunnelTimestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQTunnelMessages_TunnelTimestamp",
                table: "EQTunnelMessages");

            migrationBuilder.DropColumn(
                name: "TotalAuctionAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast30DaysAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast60DaysAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast6MonthsAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLast90DaysAverage",
                table: "EQitems");

            migrationBuilder.DropColumn(
                name: "TotalLastYearAverage",
                table: "EQitems");
        }
    }
}

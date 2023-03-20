using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class ChangedItemWTSColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalLastYearCount",
                table: "EQitems",
                newName: "TotalWTSLastYearCount");

            migrationBuilder.RenameColumn(
                name: "TotalLastYearAverage",
                table: "EQitems",
                newName: "TotalWTSLastYearAverage");

            migrationBuilder.RenameColumn(
                name: "TotalLast90DaysCount",
                table: "EQitems",
                newName: "TotalWTSLast90DaysCount");

            migrationBuilder.RenameColumn(
                name: "TotalLast90DaysAverage",
                table: "EQitems",
                newName: "TotalWTSLast90DaysAverage");

            migrationBuilder.RenameColumn(
                name: "TotalLast6MonthsCount",
                table: "EQitems",
                newName: "TotalWTSLast6MonthsCount");

            migrationBuilder.RenameColumn(
                name: "TotalLast6MonthsAverage",
                table: "EQitems",
                newName: "TotalWTSLast6MonthsAverage");

            migrationBuilder.RenameColumn(
                name: "TotalLast60DaysCount",
                table: "EQitems",
                newName: "TotalWTSLast60DaysCount");

            migrationBuilder.RenameColumn(
                name: "TotalLast60DaysAverage",
                table: "EQitems",
                newName: "TotalWTSLast60DaysAverage");

            migrationBuilder.RenameColumn(
                name: "TotalLast30DaysCount",
                table: "EQitems",
                newName: "TotalWTSLast30DaysCount");

            migrationBuilder.RenameColumn(
                name: "TotalLast30DaysAverage",
                table: "EQitems",
                newName: "TotalWTSLast30DaysAverage");

            migrationBuilder.RenameColumn(
                name: "TotalAuctionCount",
                table: "EQitems",
                newName: "TotalWTSAuctionCount");

            migrationBuilder.RenameColumn(
                name: "TotalAuctionAverage",
                table: "EQitems",
                newName: "TotalWTSAuctionAverage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalWTSLastYearCount",
                table: "EQitems",
                newName: "TotalLastYearCount");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLastYearAverage",
                table: "EQitems",
                newName: "TotalLastYearAverage");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast90DaysCount",
                table: "EQitems",
                newName: "TotalLast90DaysCount");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast90DaysAverage",
                table: "EQitems",
                newName: "TotalLast90DaysAverage");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast6MonthsCount",
                table: "EQitems",
                newName: "TotalLast6MonthsCount");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast6MonthsAverage",
                table: "EQitems",
                newName: "TotalLast6MonthsAverage");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast60DaysCount",
                table: "EQitems",
                newName: "TotalLast60DaysCount");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast60DaysAverage",
                table: "EQitems",
                newName: "TotalLast60DaysAverage");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast30DaysCount",
                table: "EQitems",
                newName: "TotalLast30DaysCount");

            migrationBuilder.RenameColumn(
                name: "TotalWTSLast30DaysAverage",
                table: "EQitems",
                newName: "TotalLast30DaysAverage");

            migrationBuilder.RenameColumn(
                name: "TotalWTSAuctionCount",
                table: "EQitems",
                newName: "TotalAuctionCount");

            migrationBuilder.RenameColumn(
                name: "TotalWTSAuctionAverage",
                table: "EQitems",
                newName: "TotalAuctionAverage");
        }
    }
}

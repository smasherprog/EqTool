using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddWikiCacheTableIndexAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_P99WikiByNames_Name",
                table: "P99WikiByNames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_P99WikiByNames_Name_ZoneName",
                table: "P99WikiByNames",
                columns: new[] { "Name", "ZoneName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_P99WikiByNames_Name",
                table: "P99WikiByNames");

            migrationBuilder.DropIndex(
                name: "IX_P99WikiByNames_Name_ZoneName",
                table: "P99WikiByNames");
        }
    }
}

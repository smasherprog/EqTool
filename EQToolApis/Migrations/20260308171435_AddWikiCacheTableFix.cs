using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddWikiCacheTableFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ZoneName",
                table: "P99WikiByNames",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldMaxLength: 48);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ZoneName",
                table: "P99WikiByNames",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldMaxLength: 48,
                oldNullable: true);
        }
    }
}

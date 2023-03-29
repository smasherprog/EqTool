using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerMessageTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<string>(
                name: "AlertType",
                table: "ServerMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ServerMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "AlertType",
                table: "ServerMessages");

            _ = migrationBuilder.DropColumn(
                name: "Title",
                table: "ServerMessages");
        }
    }
}

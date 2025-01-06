using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EQToolApis.Migrations
{
    /// <inheritdoc />
    public partial class IndexAddOnnotableNPCTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EQNotableActivities_Server_IsDeath_ActivityTime",
                table: "EQNotableActivities",
                columns: new[] { "Server", "IsDeath", "ActivityTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EQNotableActivities_Server_IsDeath_ActivityTime",
                table: "EQNotableActivities");
        }
    }
}

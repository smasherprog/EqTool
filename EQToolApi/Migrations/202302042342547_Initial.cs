using System.Data.Entity.Migrations;

namespace EQToolApi.Migrations
{
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            _ = CreateTable(
                "dbo.Players",
                c => new
                {
                    Name = c.String(nullable: false, maxLength: 24, unicode: false),
                    Server = c.Byte(nullable: false),
                    PlayerClass = c.Byte(nullable: false),
                    Level = c.Byte(nullable: false),
                })
                .PrimaryKey(t => new { t.Name, t.Server });
        }

        public override void Down()
        {
            DropTable("dbo.Players");
        }
    }
}

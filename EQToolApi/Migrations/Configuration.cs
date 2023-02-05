using System.Data.Entity.Migrations;

namespace EQToolApi.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<EQToolApi.DB.EQToolContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(EQToolApi.DB.EQToolContext context)
        {
        }
    }
}

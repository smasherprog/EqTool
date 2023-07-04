
namespace EQToolApis.Mappers
{
    public static class PlayerControllerMappers
    {
        public static List<EQToolShared.APIModels.PlayerControllerModels.Player> Map(this List<DB.Models.Player> dbplayers)
        {
            return dbplayers.Select(a => new EQToolShared.APIModels.PlayerControllerModels.Player
            {
                GuildName = a.GuildName,
                Level = a.Level,
                Name = a.Name,
                PlayerClass = a.PlayerClass,
                Server = a.Server
            }).ToList();
        }
    }
}

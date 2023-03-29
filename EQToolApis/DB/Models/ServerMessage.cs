using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(Id))]
    public class ServerMessage
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AlertType { get; set; }
        public string Message { get; set; }
    }
}
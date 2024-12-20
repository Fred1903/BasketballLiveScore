using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.DTO
{
    public class TeamDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Coach { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.Models
{
    public class Team
    {

        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Required]
        public string? Coach { get; set; }

        [Required]
        public List<Player> Players { get; set; } = new List<Player>();
    }
}

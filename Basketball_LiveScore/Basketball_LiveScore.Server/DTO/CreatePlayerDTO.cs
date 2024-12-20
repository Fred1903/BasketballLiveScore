using Basketball_LiveScore.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.DTO
{
    public class CreatePlayerDTO
    {
        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Required]
        [Range(0, 99)]
        public int Number { get; set; }

        [Required]
        public PlayerPosition Position { get; set; }

        [Required]
        [Range(1.5, 2.5)]
        public Double Height { get; set; }
        [Required]
        [MaxLength(50)]
        public string? Team { get; set; }
    }
}

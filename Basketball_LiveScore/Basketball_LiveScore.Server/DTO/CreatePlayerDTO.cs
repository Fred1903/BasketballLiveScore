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
        [MaxLength(2)]
        public int Number { get; set; }

        [Required]
        [MaxLength(20)]
        public PlayerPosition Position { get; set; }

        [Required]
        [MaxLength(3)]
        public Double Heigth { get; set; }

        //public int teamId { get; set; }
    }
}

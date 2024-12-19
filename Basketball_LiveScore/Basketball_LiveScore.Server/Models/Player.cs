using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Basketball_LiveScore.Server.Models
{
    public class Player
    {
        public int Id { get; set; }

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

        [Required]
        public Team Team { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();
    }
}

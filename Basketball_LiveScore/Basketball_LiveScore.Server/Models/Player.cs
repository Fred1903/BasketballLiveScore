using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Basketball_LiveScore.Server.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string ?FirstName { get; set; }

        [Required]
        [MaxLength(30)]
        public string ?LastName { get; set; }

        [Required]
        [Range(0, 99)]
        public int Number { get; set; }

        [Required]
        public PlayerPosition Position { get; set; }

        [Required]
        [Range(1.5, 2.5)]
        public Double Height { get; set; }

        [Required]
        public string ?Team { get; set; }

        /*public Team ?Team { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();*/
    }
}

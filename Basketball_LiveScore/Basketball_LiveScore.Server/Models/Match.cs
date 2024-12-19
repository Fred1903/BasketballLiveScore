using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.Models
{
    public class Match
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(10)]
        public DateTime matchDate { get; set; }

        
        [Required]
        public Team HomeTeam { get; set; }
        [Required]
        public Team AwayTeam { get; set; }
    }
}

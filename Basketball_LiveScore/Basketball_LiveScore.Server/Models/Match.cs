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

        [Required]
        public List<Quarter> Quarters { get; set; } // Each match will have multiple quarters

        [Required]
        public List<Timeout> Timeouts { get; set; } // Timeouts taken during the match

        [Required]
        public List<int> HomeTeamStartingFiveIds { get; set; } 
        [Required]
        public List<int> AwayTeamStartingFiveIds { get; set; }

        [Required]
        public int EncoderSettingsId { get; set; } // User who managed this data
        [Required]
        public int EncoderRealTimeId { get; set; }

        public int ScoreHome { get; set; } 
        public int ScoreAway { get; set; }
        public List<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace Basketball_LiveScore.Server.Models
{
    public class Match //Si ya le temps, créer une table MatchTeam pour diviser les responsabilités
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(10)]
        public DateTime MatchDate { get; set; }

        
        [Required]
        public Team HomeTeam { get; set; }
        [Required]
        public Team AwayTeam { get; set; }

        [Required]
        public List<Quarter> Quarters { get; set; } // Each match will have multiple quarters

        [Required]
        public List<Timeout> Timeouts { get; set; } // Timeouts taken during the match
        public int HomeTeamRemainingTimeouts { get; set; }
        public int AwayTeamRemainingTimeouts { get; set; }
        [Required]
        public List<int> HomeTeamStartingFiveIds { get; set; } 
        [Required]
        public List<int> AwayTeamStartingFiveIds { get; set; }
        //On veut aussi avoir les joueurs sur le terrain dans Match car si c'était dans 
        //MatchEvent, et que ya bcp d'event alors c'est compliqué de chercher a chaque fois les joueurs
        public List<int> HomeTeamOnFieldIds { get; set; } = new List<int>();
        public List<int> AwayTeamOnFieldIds { get; set; } = new List<int>();

        public string EncoderSettingsId { get; set; } // User who managed this data
        public string EncoderRealTimeId { get; set; }
        //On veut connaitre le currenrQuarter sans devoir faire des requetes de fou
        public int CurrentQuarter { get; set; } 


        public int ScoreHome { get; set; } 
        public int ScoreAway { get; set; }
        public List<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
        public MatchStatus Status { get; set; } = MatchStatus.NotStarted;
    }
}

using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.DTO
{
    public class MatchDTO
    {
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public TeamMatchDTO HomeTeam { get; set; }
        public TeamMatchDTO AwayTeam { get; set; }
        public int QuarterDuration { get; set; } // Durée d'un quart-temps en minutes
        public int NumberOfQuarters { get; set; }
        public int Timeouts { get; set; } // Total des temps morts
        public int TimeoutDuration { get; set; }
        public int HomeTeamRemainingTimeouts { get; set; }
        public int AwayTeamRemainingTimeouts { get; set; }
        public int ScoreHome { get; set; }
        public int ScoreAway { get; set; }
        public List<MatchPlayerDTO> Players { get; set; } // Liste des joueurs associés au match
        public int CurrentQuarter { get; set; } 
        public int CurrentTime { get; set; } 
        public bool IsRunning { get; set; } 
        public MatchStatus MatchStatus { get; set; }
    }

}

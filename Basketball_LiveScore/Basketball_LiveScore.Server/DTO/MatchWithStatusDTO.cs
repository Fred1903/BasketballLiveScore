using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.DTO
{
    public class MatchWithStatusDTO
    {
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public TeamMatchDTO HomeTeam { get; set; }
        public TeamMatchDTO AwayTeam { get; set; }
        public int ScoreHome { get; set; }
        public int ScoreAway { get; set; }
        public MatchStatus Status { get; set; }
        public int CurrentQuarter { get; set; }
        public int RemainingTime { get; set; }
        public string EncoderRealTimeId { get; set; }
    }
}

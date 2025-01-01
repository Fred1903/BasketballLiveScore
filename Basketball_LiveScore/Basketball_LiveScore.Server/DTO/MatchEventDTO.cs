using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.DTO
{
    public abstract class MatchEventDTO
    {
        public int MatchId { get; set; }
        public int Quarter { get; set; }
        public TimeSpan Time { get; set; }
    }
}

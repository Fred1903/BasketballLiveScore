using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.DTO
{
    public class MatchEventDTO
    {
        public int MatchId { get; set; }
        //public int? PlayerId { get; set; } // Optionnel pour les fautes ou paniers
        public int Quarter { get; set; }
        public TimeSpan Time { get; set; }
    }
}

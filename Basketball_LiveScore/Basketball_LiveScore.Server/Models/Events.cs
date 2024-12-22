namespace Basketball_LiveScore.Server.Models
{
    public abstract class MatchEvent
    {
        public int Id { get; set; }
        public int MatchId { get; set; } // Lié au match
        public Match Match { get; set; }
        public int Quarter { get; set; } // Quart-temps de l'événement
        public TimeSpan Time { get; set; } // Chrono au moment de l'événement

        //public abstract string GetDetails();
    }
    public class FoulEvent : MatchEvent
    {
        public int PlayerId { get; set; } 
        public string FoulType { get; set; } //(P0, P1,P2,P3)
    }

    public class BasketEvent : MatchEvent
    {
        public int PlayerId { get; set; } 
        public int Points { get; set; } 
    }
    public class SubstitutionEvent : MatchEvent
    {
        public int PlayerInId { get; set; } 
        public int PlayerOutId { get; set; } 
    }
    public class TimeoutEvent : MatchEvent
    {
        public string Team { get; set; } //equipe demandant le timeout
    }
    public class ChronoEvent : MatchEvent
    {
        public bool IsRunning { get; set; } //Vrai si chrono est en cours
    }
    public class QuarterChangeEvent : MatchEvent
    {
    }

}

namespace Basketball_LiveScore.Server.Models
{
    public class Timeout
    {
        public int Id { get; set; }
        public int MatchId { get; set; } // Foreign key to associate with a match
        public Match Match { get; set; }
        public int QuarterNumber { get; set; } // Quarter in which the timeout occurred
        public TimeSpan TimeStamp { get; set; } // When the timeout occurred within the quarter
        public string Team { get; set; } // Team that called the timeout

        public TimeOutDuration Duration { get; set; } // Duration of the timeout

        public Timeout()
        {
            Duration = TimeOutDuration.SixtySeconds; //Par défaut 60 sec
        }

    }

}

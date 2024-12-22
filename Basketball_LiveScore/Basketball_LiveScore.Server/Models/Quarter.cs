namespace Basketball_LiveScore.Server.Models
{
    public class Quarter
    {
        public int Id { get; set; }
        public int MatchId { get; set; } // Foreign key to associate with a match
        public Match Match { get; set; }
        public NumberOfQuarters Number { get; set; } 
        public QuarterDuration Duration { get; set; } 
        public TimeSpan RemainingTime { get; set; }

        public Quarter()
        {
            Number = NumberOfQuarters.Four;//Par défaut 4
            Duration = QuarterDuration.TenMinutes;//Par défaut 10
            RemainingTime = TimeSpan.FromMinutes((int)Duration);//Valeur de début est celle de Duration 
        }

    }

}

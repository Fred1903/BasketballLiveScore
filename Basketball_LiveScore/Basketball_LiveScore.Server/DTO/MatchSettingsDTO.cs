namespace Basketball_LiveScore.Server.DTO
{
    public class MatchSettingsDTO
    {
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public List<MatchPlayerDTO> Players { get; set; } // Tous les joueurs, avec indication de l'équipe
        public int NumberOfQuarters { get; set; }
        public int QuarterDuration { get; set; }
        public int TimeoutDuration { get; set; }
    }


}

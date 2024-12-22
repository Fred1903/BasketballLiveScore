﻿namespace Basketball_LiveScore.Server.DTO
{
    public class CreateMatchDTO
    {
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public int NumberOfQuarters { get; set; }
        public int QuarterDuration { get; set; }
        public int TimeoutDuration { get; set; }
        public int EncoderSettingsId { get; set; }
        public int EncoderRealTimeId { get; set; }
        public List<int> PlayersTeam1 { get; set; }
        public List<int> PlayersTeam2 { get; set; }
        public List<int> StartersTeam1 { get; set; }
        public List<int> StartersTeam2 { get; set; }
    }
}

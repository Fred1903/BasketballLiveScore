﻿namespace Basketball_LiveScore.Server.DTO
{
    public class MatchPlayerDTO
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int PlayerNumber { get; set; }
        public bool IsStarter { get; set; }
        public bool IsHomeTeam { get; set; }
        public int Fouls { get; set; } 
        public int Points { get; set; }
        public bool OnField { get; set; }
    }
}

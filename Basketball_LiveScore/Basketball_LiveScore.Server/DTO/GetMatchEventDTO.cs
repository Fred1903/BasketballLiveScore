namespace Basketball_LiveScore.Server.DTO
{
    public class GetMatchEventDTO
    {
        public string EventType { get; set; }
        public int Quarter { get; set; }
        public TimeSpan Time { get; set; }
        public int? PlayerId { get; set; }
        public int? Points { get; set; }
        public string FoulType { get; set; }
        public int? PlayerInId { get; set; }
        public int? PlayerOutId { get; set; }
        public string Team { get; set; }
        public bool? IsRunning { get; set; }
    }
}

namespace Basketball_LiveScore.Server.DTO
{
    public class SubstitutionEventDTO : MatchEventDTO
    {
        public int PlayerInId { get; set; }
        public int PlayerOutId { get; set; }
    }
}

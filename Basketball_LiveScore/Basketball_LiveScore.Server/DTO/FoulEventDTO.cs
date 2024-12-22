namespace Basketball_LiveScore.Server.DTO
{
    public class FoulEventDTO : MatchEventDTO
    {
        public int PlayerId { get; set; }
        public string FoulType { get; set; }
    }
}

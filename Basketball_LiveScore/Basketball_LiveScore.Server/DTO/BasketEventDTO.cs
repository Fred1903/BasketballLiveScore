namespace Basketball_LiveScore.Server.DTO
{
    public class BasketEventDTO : MatchEventDTO
    {
        public int PlayerId { get; set; }
        public int Points { get; set; }
    }
}

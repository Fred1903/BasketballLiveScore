namespace Basketball_LiveScore.Server.DTO
{
    public class AddFoulDTO
    {
        public int PlayerId { get; set; }
        public int Quarter { get; set; }
        public string FoulType { get; set; } 
        public TimeSpan Time { get; set; }
    }
}

namespace Basketball_LiveScore.Server.DTO
{
    public class TeamMatchDTO
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string Coach { get; set; }
        public int Score { get; set; } 
        public List<int> StartingFive { get; set; } 
    }
}

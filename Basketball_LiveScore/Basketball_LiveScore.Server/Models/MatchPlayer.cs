namespace Basketball_LiveScore.Server.Models
{
    public class MatchPlayer
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public bool IsStarter { get; set; } //Si est titu
        public bool IsHomeTeam { get; set; }//booleen pr savoir si il est dans equipe adverse ou home
    }

}

namespace Basketball_LiveScore.Server.DTO
{
    public class PlayerDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
        public string Position { get; set; }
        public double Height { get; set; }
        //Pour l instant teamId pas nécessaire mais si un autre endpoint utilise playerDTO alors si
        //public int TeamId {  get; set; } 
    }

}

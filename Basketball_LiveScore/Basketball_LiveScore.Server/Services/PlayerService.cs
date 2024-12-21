using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly BasketballDBContext basketballDBContext;

        public PlayerService(BasketballDBContext basketballDBContext)
        {
            this.basketballDBContext = basketballDBContext;
        }

        public async Task<Player> CreatePlayerAsync(CreatePlayerDTO createPlayerDto)
        {
            var player = new Player
            {
                FirstName = createPlayerDto.FirstName,
                LastName = createPlayerDto.LastName,
                Number = createPlayerDto.Number,
                Position = createPlayerDto.Position,
                Height = createPlayerDto.Height,
                TeamId = basketballDBContext.Teams.FirstOrDefault(t => t.Name.Equals(createPlayerDto.Team)).Id,
            };

            basketballDBContext.Players.Add(player);
            await basketballDBContext.SaveChangesAsync();

            return player;
        }
    }
}

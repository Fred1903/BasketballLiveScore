using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Microsoft.EntityFrameworkCore;


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
            var team = await basketballDBContext.Teams.FirstOrDefaultAsync(t => t.Name == createPlayerDto.Team);

            if(basketballDBContext.Players.Any(p=>p.Team.Name==createPlayerDto.Team &&
                p.Number == createPlayerDto.Number))
            {
                throw new ArgumentException("There is already a player with the same number in the team, please choose another one");
            }

            var player = new Player
            {
                FirstName = createPlayerDto.FirstName,
                LastName = createPlayerDto.LastName,
                Number = createPlayerDto.Number,
                Position = createPlayerDto.Position,
                Height = createPlayerDto.Height,
                TeamId = team.Id,
            };

            basketballDBContext.Players.Add(player);
            await basketballDBContext.SaveChangesAsync();

            return player;
        }
    }
}

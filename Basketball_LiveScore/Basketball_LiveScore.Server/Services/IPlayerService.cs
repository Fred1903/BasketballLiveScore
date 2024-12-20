using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Services
{
    public interface IPlayerService
    {
        Task<Player> CreatePlayerAsync(CreatePlayerDTO createPlayerDto);
    }
}
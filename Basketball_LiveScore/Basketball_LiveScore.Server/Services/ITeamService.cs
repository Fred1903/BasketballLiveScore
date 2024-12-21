using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Services
{
    public interface ITeamService
    {
        Task<Team> CreateTeam(CreateTeamDTO createTeamDTO);
        Task<List<TeamDTO>> GetTeams();
        Task<TeamDTO> GetTeam(int teamId);
    }
}
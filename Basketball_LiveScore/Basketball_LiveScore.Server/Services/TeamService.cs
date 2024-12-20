using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Basketball_LiveScore.Server.Services
{
    public class TeamService : ITeamService
    {
        private readonly BasketballDBContext basketballDBContext;

        public TeamService(BasketballDBContext basketballDBContext)
        {
            this.basketballDBContext = basketballDBContext;
        }

        public async Task<Team> CreateTeam(CreateTeamDTO createTeamDTO)
        {
            if (basketballDBContext.Teams.Any(t => t.Name == createTeamDTO.Name))
            {
                throw new InvalidOperationException($"A team with the name '{createTeamDTO.Name}' already exists.");
            }

            Team team = new()
            {
                Name = createTeamDTO.Name,
                Coach = createTeamDTO.Coach
            };

            basketballDBContext.Teams.Add(team);
            await basketballDBContext.SaveChangesAsync();
            Console.WriteLine($"New Team Created with Id: {team.Id}");
            return team;
        }

        public async Task<List<TeamDTO>> GetTeams()
        {
            return await basketballDBContext.Teams
                .Select(team => new TeamDTO
                {
                    Id = team.Id,
                    Name = team.Name,
                    Coach = team.Coach
                })
                .ToListAsync();
        }
    }
}

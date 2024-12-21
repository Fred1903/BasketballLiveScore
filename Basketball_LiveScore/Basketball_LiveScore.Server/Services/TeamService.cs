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

        public async Task<TeamDTO> GetTeam(int teamId)
        {
            var team=  await basketballDBContext.Teams
                .Include(t => t.Players) // Inclut les joueurs associés
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
            {
                throw new KeyNotFoundException($"Team with ID {teamId} not found.");
            }

            return MapToTeamDTO(team); //on ne renvoie un DTO et pas l'objet directement !
        }

        private TeamDTO MapToTeamDTO(Team team)
        {
            return new TeamDTO
            {
                Id = team.Id,
                Name = team.Name,
                Coach = team.Coach,
                Players = team.Players.Select(p => new PlayerDTO
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Number = p.Number,
                    Position = p.Position.ToString(), // Enum -> String
                    Height = p.Height
                }).ToList()
            };
        }


    }
}

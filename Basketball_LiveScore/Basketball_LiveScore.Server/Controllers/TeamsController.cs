namespace Basketball_LiveScore.Server.Controllers
{
    using global::Basketball_LiveScore.Server.DTO;
    using global::Basketball_LiveScore.Server.Models;
    using global::Basketball_LiveScore.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    namespace Basketball_LiveScore.Server.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class TeamsController : ControllerBase
        {
            private readonly ITeamService teamService;

            public TeamsController(ITeamService teamService)
            {
                this.teamService = teamService;
            }

            [Authorize(Policy = "AdminOnly")]
            [HttpPost]
            [Route("create")]
            public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDTO createTeamDTO)
            {
                if (!ModelState.IsValid) //on regarde si les [required],[mayLength] sont respectés
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    Team createdTeam = await teamService.CreateTeam(createTeamDTO);
                    return StatusCode(201, createdTeam);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating team: {ex.Message}");
                    return StatusCode(500, "An error occurred while creating the team.");
                }
            }

            [Authorize(Policy = "AuthenticatedUsers")]
            [HttpGet]
            [Route("all")]
            public async Task<IActionResult> GetTeams()
            {
                try
                {
                    var teams = await teamService.GetTeams();
                    return Ok(teams);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving teams: {ex.Message}");
                    return StatusCode(500, "An error occurred while retrieving the teams.");
                }
            }

            [Authorize(Policy = "AuthenticatedUsers")]
            [HttpGet("{teamId}")]
            public async Task<IActionResult> GetTeam(int teamId)
            {
                try
                {
                    var teamDto = await teamService.GetTeam(teamId);
                    return Ok(teamDto);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }

        }
    }
}

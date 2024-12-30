using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Basketball_LiveScore.Server.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace Basketball_LiveScore.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService playerService;

        public PlayersController(IPlayerService playerService)
        {
            this.playerService = playerService;
        }

        //[Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDTO playerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdPlayer = await playerService.CreatePlayerAsync(playerDto);
                return StatusCode(201, createdPlayer);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("playersOfTeam")]


        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("positions")]
        public IActionResult GetPositions()
        {//pour pouvoir use p.GetDescription obligé d'importer Utilities
            var positions = Enum.GetValues(typeof(PlayerPosition))
                                .Cast<PlayerPosition>()
                                .Select(p => new { Value = p.ToString(), Display = p.GetDescription() })
                                .ToList();

            return Ok(positions);
        }
    }
}

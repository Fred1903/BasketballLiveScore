﻿using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Basketball_LiveScore.Server.Utilities;

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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

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
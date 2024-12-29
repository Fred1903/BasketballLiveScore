using Microsoft.AspNetCore.Mvc;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Basketball_LiveScore.Server.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Basketball_LiveScore.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService matchService;

        public MatchController(IMatchService matchService)
        {
            this.matchService = matchService;
        }

        [HttpPost("create")]
        public IActionResult CreateMatch([FromBody] CreateMatchDTO matchDTO)
        {
            try
            {
                var createdMatch = matchService.CreateMatch(matchDTO);
                return CreatedAtAction(nameof(CreateMatch), new { id = createdMatch.Id }, createdMatch);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, "An error occurred while creating the match.");
            }
        }

        [HttpPost("start/{matchId}")]
        public IActionResult StartMatch(int matchId)
        {
            try
            {
                matchService.StartMatch(matchId);
                return Ok(new { message = "Match started successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("finish/{matchId}")]
        public async Task<IActionResult> FinishMatch(int matchId)
        {
            try
            {
                matchService.FinishMatch(matchId);
                return Ok(new { message = "Match finished successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("/events/{matchId}")]
        public IActionResult GetMatchEvents(int matchId)
        {
            var events = matchService.GetMatchEvents(matchId);
            return Ok(events);
        }   

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("settings/number-of-quarters")]
        public ActionResult<List<int>> GetNumberOfQuartersOptions()
        {
            var options = matchService.GetNumberOfQuartersOptions();
            return Ok(options);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("settings/quarter-durations")]
        public ActionResult<List<int>> GetQuarterDurationOptions()
        {
            var options = matchService.GetQuarterDurationOptions();
            return Ok(options);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("settings/timeout-durations")]
        public ActionResult<List<int>> GetTimeOutDurationOptions()
        {
            var options = matchService.GetTimeOutDurationOptions();
            return Ok(options);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("settings/timeout-amount")]
        public ActionResult<List<int>> GetTimeOutAmountOptions()
        {
            var options = matchService.GetTimeOutAmountOptions();
            return Ok(options);
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpGet("settings/defaults")]
        public IActionResult GetDefaultSettings()
        {
            var defaults = matchService.GetDefaultSettings();
            return Ok(defaults);
        }

        [HttpGet("{matchId}")]
        public IActionResult GetMatchDetails(int matchId)
        {
            try
            {
                var matchDetails = matchService.GetMatchDetails(matchId);
                return Ok(matchDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Ci-dessous : endpoints des events
        [HttpPost("{matchId}/add-foul")]
        public async Task<IActionResult> AddFoulEvent(int matchId, [FromBody] FoulEventDTO foulEventDTO)
        {
            try
            {
                await matchService.AddFoulEvent(matchId, foulEventDTO);
                return Ok(new { Message = "Foul event added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-basket")]
        public async Task<IActionResult> AddBasketEvent(int matchId, [FromBody] BasketEventDTO basketEventDTO)
        {
            try
            {
                if (basketEventDTO == null)
                {
                    throw new Exception("BasketEventDTO is null.");
                }

                if (basketEventDTO.Time == TimeSpan.Zero)
                {
                    throw new Exception("Time cannot be zero.");
                }

                await matchService.AddBasketEvent(matchId, basketEventDTO);
                return Ok(new { Message = "Basket event added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-substitution")]
        public async Task<IActionResult> AddSubstitutionEvent(int matchId, [FromBody] SubstitutionEventDTO substitutionEventDTO)
        {
            try
            {
                await matchService.AddSubstitutionEvent(matchId, substitutionEventDTO);
                return Ok(new { Message = "Substitution event added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-timeout")]
        public async Task<IActionResult> AddTimeoutEvent(int matchId, [FromBody] TimeoutEventDTO timeoutEventDTO)
        {
            try
            {
                await matchService.AddTimeoutEvent(matchId, timeoutEventDTO);
                return Ok(new { Message = "Timeout event added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-chrono")]
        public async Task<IActionResult> AddChronoEvent(int matchId, [FromBody] ChronoEventDTO chronoEventDTO)
        {
            try
            {
                await matchService.AddChronoEvent(matchId, chronoEventDTO);
                return Ok(new { Message = "Chrono event added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-quarter-change")]
        public async Task<IActionResult> AddQuarterChangeEvent(int matchId, [FromBody] QuarterChangeEventDTO quarterChangeEventDTO)
        {
            try
            {
                await matchService.AddQuarterChangeEvent(matchId, quarterChangeEventDTO);
                return Ok(new { Message = "Quarter event added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpGet("foul-types")]
        public IActionResult GetFoulTypes()
        {
            int i = 4;
            var foulTypes = Enum.GetValues(typeof(FoulTypes))
                .Cast<FoulTypes>()
                .Select(f => new { Id = (int)f, Name = f.ToString() })
                .ToList();

            return Ok(foulTypes);
        }

        [HttpGet("basket-points")]
        public IActionResult GetBasketPoints()
        {
            var points = Enum.GetValues(typeof(BasketPoints))
                .Cast<BasketPoints>()
                .Select(p => new { Id = (int)p })  //on envoie que 1,2,3    
                .ToList();

            return Ok(points);
        }

        [HttpGet("all")]
        public IActionResult GetAllMatchesWithStatus()
        {
            try
            {
                var matches = matchService.GetAllMatchesWithStatus();
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}

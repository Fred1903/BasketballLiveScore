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

        [Authorize(Policy = "AdminAndEncoder")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchDTO matchDTO)
        {
            try
            {
                var createdMatch = await matchService.CreateMatch(matchDTO);
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

        [Authorize(Policy = "AdminAndEncoder")]
        [HttpPost("start/{matchId}")]
        public async Task<IActionResult> StartMatch(int matchId)
        {
            try
            {
                await matchService.StartMatch(matchId);
                return Ok(new { message = "Match started successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize(Policy = "AdminAndEncoder")]
        [HttpPut("finish/{matchId}")]
        public async Task<IActionResult> FinishMatch(int matchId)
        {
            try
            {
                await matchService.FinishMatch(matchId);
                return Ok(new { message = "Match finished successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [Authorize(Policy = "AdminAndEncoder")]
        [HttpGet("settings/number-of-quarters")]
        public ActionResult<List<int>> GetNumberOfQuartersOptions()
        {
            var options = matchService.GetNumberOfQuartersOptions();
            return Ok(options);
        }


        [Authorize(Policy = "AdminAndEncoder")]
        [HttpGet("settings/quarter-durations")]
        public ActionResult<List<int>> GetQuarterDurationOptions()
        {
            var options = matchService.GetQuarterDurationOptions();
            return Ok(options);
        }


        [Authorize(Policy = "AdminAndEncoder")]
        [HttpGet("settings/timeout-durations")]
        public ActionResult<List<int>> GetTimeOutDurationOptions()
        {
            var options = matchService.GetTimeOutDurationOptions();
            return Ok(options);
        }


        [Authorize(Policy = "AdminAndEncoder")]
        [HttpGet("settings/timeout-amount")]
        public ActionResult<List<int>> GetTimeOutAmountOptions()
        {
            var options = matchService.GetTimeOutAmountOptions();
            return Ok(options);
        }


        [Authorize(Policy = "AdminAndEncoder")]
        [HttpGet("settings/defaults")]
        public IActionResult GetDefaultSettings()
        {
            var defaults = matchService.GetDefaultSettings();
            return Ok(defaults);
        }

        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatchDetails(int matchId)
        {
            try
            {
                var matchDetails = await matchService.GetMatchDetails(matchId);
                return Ok(matchDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Ci-dessous : endpoints des events
        [Authorize(Policy = "AdminAndEncoder")]
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

        [Authorize(Policy = "AdminAndEncoder")]
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

        [Authorize(Policy = "AdminAndEncoder")]
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

        [Authorize(Policy = "AdminAndEncoder")]
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

        [Authorize(Policy = "AdminAndEncoder")]
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

        [Authorize(Policy = "AdminAndEncoder")]
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


        [Authorize(Policy = "AdminAndEncoder")]
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


        [Authorize(Policy = "AdminAndEncoder")]
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
        public async Task<IActionResult> GetAllMatchesWithStatus()
        {
            try
            {
                var matches = await matchService.GetAllMatchesWithStatus();
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("events/{matchId}")]
        public async Task<IActionResult> GetAllEvents(int matchId)
        {
            try
            {
                var events = await matchService.GetMatchEvents(matchId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Basketball_LiveScore.Server.DTO;

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
                return BadRequest(ex.Message);
            }
        }

        /*[HttpPost("/events/add")]
        public IActionResult AddMatchEvent([FromBody] MatchEventDTO matchEventDto)
        {
            var matchEvent = matchService.AddMatchEvent(matchEventDto);
            return Ok(matchEvent);
        }*/

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

                if (matchDetails == null)
                {
                    return NotFound($"Match with ID {matchId} not found.");
                }

                return Ok(matchDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Ci-dessous : endpoints des events

        [HttpPost("{matchId}/add-foul")]
        public IActionResult AddFoulEvent(int matchId, [FromBody] FoulEventDTO foulEventDto)
        {
            try
            {
                var matchEvent = matchService.AddFoulEvent(matchId, foulEventDto);
                return Ok(matchEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-basket")]
        public IActionResult AddBasketEvent(int matchId, [FromBody] BasketEventDTO basketEventDto)
        {
            try
            {
                var matchEvent = matchService.AddBasketEvent(matchId, basketEventDto);
                return Ok(matchEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-substitution")]
        public IActionResult AddSubstitutionEvent(int matchId, [FromBody] SubstitutionEventDTO substitutionEventDto)
        {
            try
            {
                var matchEvent = matchService.AddSubstitutionEvent(matchId, substitutionEventDto);
                return Ok(matchEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-timeout")]
        public IActionResult AddTimeoutEvent(int matchId, [FromBody] TimeoutEventDTO timeoutEventDto)
        {
            try
            {
                var matchEvent = matchService.AddTimeoutEvent(matchId, timeoutEventDto);
                return Ok(matchEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{matchId}/add-chrono")]
        public IActionResult AddChronoEvent(int matchId, [FromBody] ChronoEventDTO chronoEventDto)
        {
            try
            {
                var matchEvent = matchService.AddChronoEvent(matchId, chronoEventDto);
                return Ok(matchEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

    }
}

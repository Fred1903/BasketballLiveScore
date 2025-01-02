using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;

namespace Basketball_LiveScore.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        
        [Authorize(Policy = "AdminAndEncoder")]
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await userService.GetUsersByRole(role);

                if (users == null || !users.Any())
                {//On renvoie une liste vide pas une erreur car comme ca affiche juste vide 
                 //la ou ya les encodeurs ou users pour admin car apres peut faire erreur sinon cote front
                    return Ok(new List<UserDTO>());
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("add-encoder/{idUser}")]
        public async Task<IActionResult> AddUserAsEncoder(Guid idUser)
        {
            try
            {
                await userService.AddUserAsEncoder(idUser);
                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("remove-encoder/{idUser}")]
        public async Task<IActionResult> RemoveUserFromsEncoders(Guid idUser)
        {
            try
            {
                await userService.RemoveUserFromEncoders(idUser);
                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

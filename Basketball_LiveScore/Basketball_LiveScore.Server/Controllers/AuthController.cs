using Basketball_LiveScore.Server.Models;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Basketball_LiveScore.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IUserService userService;
        private readonly IAuthService authService;

        public AuthController(IConfiguration configuration, IUserService userService, IAuthService authService)
        {
            this.configuration = configuration;
            this.userService = userService;
            this.authService = authService;
        }

        [Authorize(Policy = "AnonymousOnly")]
        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin user)
        {
            try
            {
                var token = authService.Authenticate(user);
                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AnonymousOnly")]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegister newUser)
        {
            try
            {
                var result = await authService.RegisterUser(newUser);
                return Ok(new { message = "User registered successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
        }
    }
}

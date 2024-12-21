using Basketball_LiveScore.Server.Models;
using Basketball_LiveScore.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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

        /*[Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLogin user)
        {//IActionResult permet de controler le HTTP renvoyé avec statut
            try
            {
                if (!string.IsNullOrEmpty(user.Username)&&!string.IsNullOrEmpty(user.Password))
                {
                    var loggedInUser = userService.Get(user);
                    if (loggedInUser is null) return NotFound("User not found"); //mdp ou login ou les 2 pas bon
                    var claims = new[] 
                    {
                        new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Name,loggedInUser.Username),
                        new Claim(ClaimTypes.Email,loggedInUser.Email),
                        new Claim(ClaimTypes.GivenName,loggedInUser.FirstName),
                        new Claim(ClaimTypes.Surname,loggedInUser.LastName),//on utilise les propriétés que claim a
                        new Claim(ClaimTypes.Role,loggedInUser.Role),
                    };
                    var token = new JwtSecurityToken
                    (
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims, //on ajoute les infos du user dans le token
                    expires: DateTime.UtcNow.AddMinutes(30), //token sera valide pendant cette durée
                    notBefore: DateTime.UtcNow,//token n'est pas valide avant mtn
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256)
                    );
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(tokenString);
                }
                return BadRequest("Invalid user credentials");
            }
            catch (ArgumentException ex)
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegister newUser)
        {
            try
            {
                // Vérification des données
                if (string.IsNullOrEmpty(newUser.Username) ||string.IsNullOrEmpty(newUser.Password) ||
                    string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.FirstName) ||
                    string.IsNullOrEmpty(newUser.LastName))
                {
                    return BadRequest("Please complete all fields");
                }

                // Vérification si le username ou email existe déjà
                var existingUsername = userService.GetByUsername(newUser.Username);
                var existingEmail = userService.GetByEmail(newUser.Email);
                if (existingUsername != null)
                {
                    return Conflict("Username already exists."); // HTTP 409 Conflict
                }
                if (existingEmail != null)
                {
                    return Conflict("There is already an account with this email"); 
                }
                var hashedPassword = authService.HashPassword(newUser.Password);
                // Création de l'utilisateur
                var createdUser = new User
                {
                    Username = newUser.Username,
                    PasswordHash = hashedPassword,
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Role = "User" // Role par défaut
                };

                var result = userService.AddUser(createdUser);
                if (!result)
                {
                    return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, "User registration failed.");
                }

                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }*/


        /*[Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLogin user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new { message = "Invalid user credentials." });
            }

            var loggedInUser = userService.Get(user);
            if (loggedInUser == null || !authService.VerifyPassword(user.Password, loggedInUser.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var token = authService.GenerateJwtToken(loggedInUser);
            return Ok(new { Token = token });
        }*/

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


        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegister newUser)
        {//Verif si tous les champs sont rempli
            if (string.IsNullOrEmpty(newUser.Username) || string.IsNullOrEmpty(newUser.Password) ||
                string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.FirstName) ||
                string.IsNullOrEmpty(newUser.LastName))
            {
                return BadRequest("Please complete all fields.");
            }
            //Vérif si username ou email existe déjà
            if (userService.GetByUsername(newUser.Username) != null)
            {
                return Conflict("Username already exists.");// HTTP 409 Conflict
            }
            if (userService.GetByEmail(newUser.Email) != null)
            {
                return Conflict("There is already an account with this email.");
            }

            var hashedPassword = authService.HashPassword(newUser.Password);
            var createdUser = new User
            {
                Username = newUser.Username,
                PasswordHash = hashedPassword,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Role = "User"
            };
            var result = await userService.AddUser(createdUser);
            if (!result)
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, "User registration failed.");
            }
            return Ok("User registered successfully.");
        }
    }
}

using Basketball_LiveScore.Server.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Basketball_LiveScore.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly IUserService userService;

        public AuthService(IConfiguration configuration, IUserService userService)
        {
            this.configuration = configuration;
            this.userService = userService;
        }
        public string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),//on utilise les propriétés que claim a
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken
            (
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,//on ajoute les infos du user dans le token
                expires: DateTime.UtcNow.AddMinutes(150),//token sera valide pendant cette durée
                notBefore: DateTime.UtcNow,//token n'est pas valide avant mtn
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string Authenticate(UserLogin userLogin)
        {
            if (string.IsNullOrEmpty(userLogin.Username) || string.IsNullOrEmpty(userLogin.Password))
            {
                throw new ArgumentException("Username and password are required.");
            }

            var user = userService.Get(userLogin);
            if (user == null || !VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            return GenerateJwtToken(user);
        }

        public async Task<string> RegisterUser(UserRegister newUser)
        {
            ////Verif si tous les champs sont rempli
            if (string.IsNullOrEmpty(newUser.Username) || string.IsNullOrEmpty(newUser.Password) ||
                string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.FirstName) ||
                string.IsNullOrEmpty(newUser.LastName))
            {
                throw new ArgumentException("Please complete all fields.");
            }

            //Vérif si username ou email existe déjà
            if (userService.GetByUsername(newUser.Username) != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (userService.GetByEmail(newUser.Email) != null)
            {
                throw new InvalidOperationException("There is already an account with this email.");
            }
            var hashedPassword = HashPassword(newUser.Password);
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
                throw new Exception("User registration failed.");
            }
            return "User registered successfully.";
        }


    }
}

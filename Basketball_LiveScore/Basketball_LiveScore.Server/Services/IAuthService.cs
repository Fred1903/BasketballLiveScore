using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Services
{
    public interface IAuthService
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string hashedPassword);
        public string GenerateJwtToken(User user);
        public Task<string> Authenticate(UserLogin userLogin);
        Task<string> RegisterUser(UserRegister newUser);
    }
}
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Services
{
    public interface IUserService
    {
        public User Get(UserLogin userLogin);
        public User GetByUsername(string username);
        public User GetByEmail(string email);
        public Task<bool> AddUser(User user);

        public Task<List<UserDTO>> GetUsersByRole(string role);
    }
}

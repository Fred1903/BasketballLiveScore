using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Services
{
    public interface IUserService
    {
        public Task<User> Get(UserLogin userLogin);
        public Task<User> GetByUsername(string username);
        public Task<User> GetByEmail(string email);
        public Task<bool> AddUser(User user);
        public Task<List<UserDTO>> GetUsersByRole(string role);

        public Task AddUserAsEncoder(Guid userId);
        public Task RemoveUserFromEncoders(Guid userId);

    }
}

using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.DTO;
using Basketball_LiveScore.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Basketball_LiveScore.Server.Services
{
    public class UserService : IUserService
    {
        private BasketballDBContext basketballDBContext;
        public UserService(BasketballDBContext basketballDBContext)
        {
            this.basketballDBContext = basketballDBContext;
        }

        public async Task<bool> AddUser(User user)
        {
            try
            {
                // Ajoute l'utilisateur à la base de données
                await basketballDBContext.Users.AddAsync(user); 
                await basketballDBContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                return false;
            }
        }

        public async Task<User> Get(UserLogin userLogin)
        {
            if (userLogin == null || userLogin.Username == null || userLogin.Password==null)
            {
                throw new ArgumentNullException(nameof(userLogin), "UserLogin or its User property is null.");
            }   
            return await basketballDBContext.Users.FirstOrDefaultAsync(o => o.Username.Equals(userLogin.Username));
        }

        public async Task<User> GetByEmail(string email)
        {
            return await basketballDBContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<User> GetByUsername(string username)
        { //On renvoie le user ou null
            return await basketballDBContext.Users.FirstOrDefaultAsync(u => u.Username.Equals(username));
        }

        public async Task<List<UserDTO>> GetUsersByRole(string role)
        {
            var users = await basketballDBContext.Users.Where(u => u.Role.Equals(role))
                       .ToListAsync();

            //Ensuite on renvoie les users sous Forme de DTO
            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }).ToList();
        }
        public async Task AddUserAsEncoder(Guid userId) 
        {
            var user = basketballDBContext.Users.FirstOrDefault(u => u.Id ==userId);
            if (user==null) 
                throw new ArgumentNullException("No user found with this id");
            if (user.Role.Equals("Encoder"))
                throw new ArgumentException("User is already an encoder");
            user.Role = "Encoder";
            await basketballDBContext.SaveChangesAsync();
        }
        public async Task RemoveUserFromEncoders(Guid userId)
        {
            var user = basketballDBContext.Users.FirstOrDefault(u => u.Id==userId);
            if (user == null)
                throw new ArgumentNullException("No user found with this id");
            if (user.Role.Equals("User"))
                throw new ArgumentException("User is not an encoder");
            user.Role = "User";
            await basketballDBContext.SaveChangesAsync();
        }
    }
}

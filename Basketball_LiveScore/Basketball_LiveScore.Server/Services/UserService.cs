using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.Models;
using Microsoft.EntityFrameworkCore;

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

        public User Get(UserLogin userLogin)
        {
            if (userLogin == null || userLogin.Username == null || userLogin.Password==null)
            {
                throw new ArgumentNullException(nameof(userLogin), "UserLogin or its User property is null.");
            }   
            return basketballDBContext.Users.FirstOrDefault(o => o.Username.Equals(userLogin.Username));
        }

        public User GetByEmail(string email)
        {
            return basketballDBContext.Users.FirstOrDefault(u => u.Email.Equals(email));
        }

        public User GetByUsername(string username)
        { //On renvoie le user ou null
            return basketballDBContext.Users.FirstOrDefault(u => u.Username.Equals(username));
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UDP_ChatDataBase;
using UDP_ChatDataBase.Context;
using UDP_ChatDataBase.Entity;
using static UDP_ChatServer.Services.UserServices;

namespace UDP_ChatServer.Repositories
{
    public class UserRepository
    {
        private readonly DbContextChat _dbContext;

        public UserRepository(DbContextChat dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddChatMessage(ChatMessage chatMessage)
        {
            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Users>> GetAllUsers()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<Users> GetUserByEmail(string email)
        {
            try
            {
                return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<Users> GetUserById(int userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<ChatMessage>> SearchMessagesByUserId(int userId)
        {
            return await _dbContext.ChatMessages.Where(cm => cm.UsersId == userId).ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> SearchMessagesByEmail(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                return await _dbContext.ChatMessages.Where(cm => cm.UsersId == user.Id).ToListAsync();
            }
            else
            {
                return Enumerable.Empty<ChatMessage>();
            }
        }

        public async Task AddUser(Users user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserStatus> VerifyUser(string email, string password)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
                if (user != null)
                {
                    return user.IsAdmin ? UserStatus.Admin : UserStatus.RegularUser;
                }
                else
                {
                    return UserStatus.NotRegistered;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying user: {ex.Message}");
                return UserStatus.NotRegistered; 
            }
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        public void RemoveUser(Users user)
        {
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }
    }
}

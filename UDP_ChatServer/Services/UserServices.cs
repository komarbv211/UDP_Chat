
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UDP_ChatDataBase.Context;
using UDP_ChatServer.Repositories;
using System.Threading.Tasks;
using UDP_ChatDataBase;
using UDP_ChatDataBase.Entity;
using System.Reflection.Metadata;

namespace UDP_ChatServer.Services
{
    public static class UserServices
    {
        private static readonly UserRepository _userRepository = new UserRepository(new DbContextChat());

        public enum UserStatus
        {
            NotRegistered,
            RegularUser,
            Admin
        }

        public static string UserStatusToString(UserStatus status)
        {
            return status switch
            {
                UserStatus.NotRegistered => "NotRegistered",
                UserStatus.RegularUser => "RegularUser",
                UserStatus.Admin => "Admin",
                _ => throw new InvalidOperationException("Unknown user status")
            };
        }
        public static void ConnectUser(IPEndPoint clientEndPoint, UdpClient server)
        {
            Console.WriteLine("New client joined the chat.");
            byte[] joinMessage = Encoding.Unicode.GetBytes("<join>");
            server.Send(joinMessage, joinMessage.Length, clientEndPoint);
            Console.WriteLine("Sent <join> messages to the client.");
        }
        public static async Task SendingNotificationsToUsers(string message, UdpClient server, HashSet<IPEndPoint> members)
        {
            string[] messageParts = message.Split(':');
            string content = messageParts[1].Trim();

            byte[] data = Encoding.Unicode.GetBytes(content);
            foreach (var m in members)
            {
                await server.SendAsync(data, data.Length, m);
            }
        }
        public static async Task GetListAllUsers(IPEndPoint clientEndPoint, UdpClient server)
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                if (users.Any())
                {
                    StringBuilder userListBuilder = new StringBuilder();
                    foreach (var user in users)
                    {
                        userListBuilder.AppendLine(user.ToString());
                    }
                    string userListMessage = userListBuilder.ToString();
                    byte[] userListData = Encoding.Unicode.GetBytes(userListMessage);
                    server.Send(userListData, userListData.Length, clientEndPoint);
                }
                else
                {
                    Console.WriteLine("No users found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving users: {ex.Message}");
            }
        }
        public static async Task AddMessageToTheDataBase(string message, UdpClient server)
        {
            try
            {
                using (var dbContext = new DbContextChat())
                {
                    string[] messageParts = message.Split(':');
                    string content = messageParts[1].Trim();
                    int id = int.Parse(messageParts[2].Trim());
                    Console.WriteLine($"UserId {id}");

                    var chatMessage = new ChatMessage
                    {
                        Content = content,
                        Timestamp = DateTime.Now,
                        UsersId = id
                    };

                    await dbContext.AddAsync(chatMessage);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding message to the chat: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            }
        }
        public static async Task SendVerificationResponse(string message, IPEndPoint clientEndPoint, UdpClient server)
        {
            try
            {
                // Отримання даних з повідомлення користувача
                string[] userInfo = message.Split(':');
                string email = userInfo[1];
                string password = userInfo[2];

                // Верифікація користувача
                var verification = _userRepository.VerifyUser(email, password);

                // Конвертація результату верифікації в рядок
                string responseMessage = UserStatusToString(await verification);
                Console.WriteLine(responseMessage);

                byte[] responseData = Encoding.Unicode.GetBytes(responseMessage);
                server.Send(responseData, responseData.Length, clientEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Обробка помилок
            }
        }
        public static async Task RegisterUser(string message, IPEndPoint clientEndPoint, UdpClient server)
        {
            // Отримання даних з повідомлення користувача
            string[] userInfo = message.Split(':');
            string fullName = userInfo[1];
            string email = userInfo[2];
            string password = userInfo[3];

            try
            {
                // Створення нового об'єкта користувача
                var newUser = new Users
                {
                    FullName = fullName,
                    Email = email,
                    Password = password,
                    IsAdmin = false
                };

                // Додавання користувача до бази даних
                await _userRepository.AddUser(newUser);

                // Підтвердження успішної реєстрації користувача
                string responseMessage = "Registration successful!";
                byte[] responseData = Encoding.Unicode.GetBytes(responseMessage);
                server.Send(responseData, responseData.Length, clientEndPoint);
            }
            catch (Exception ex)
            {
                // Обробка помилок при реєстрації користувача
                string errorMessage = $"Error registering user: {ex.Message}";
                byte[] errorData = Encoding.Unicode.GetBytes(errorMessage);
                server.Send(errorData, errorData.Length, clientEndPoint);
            }
        }
        public static async Task SearchUserMessages(int userId, IPEndPoint clientEndPoint, UdpClient server)
        {
            try
            {
                // Пошук повідомлень користувача за його ідентифікатором
                var messages = await _userRepository.SearchMessagesByUserId(userId);

                if (messages.Any())
                {
                    // Створення рядка з повідомленнями користувача
                    StringBuilder messageBuilder = new StringBuilder();
                    foreach (var message in messages)
                    {
                        messageBuilder.AppendLine(message.ToString());
                    }
                    string userMessages = messageBuilder.ToString();

                    // Відправлення повідомлення клієнту
                    byte[] responseData = Encoding.Unicode.GetBytes(userMessages);
                    server.Send(responseData, responseData.Length, clientEndPoint);
                }
                else
                {
                    // Повідомлення, якщо повідомлень користувача не знайдено
                    string noMessages = "No messages found for this user.";
                    byte[] responseData = Encoding.Unicode.GetBytes(noMessages);
                    server.Send(responseData, responseData.Length, clientEndPoint);
                }
            }
            catch (Exception ex)
            {
                // Обробка помилок
                string errorMessage = $"Error searching user messages: {ex.Message}";
                byte[] errorData = Encoding.Unicode.GetBytes(errorMessage);
                server.Send(errorData, errorData.Length, clientEndPoint);
            }
        }
        public static async Task GrantAdminRights(int userId, IPEndPoint clientEndPoint, UdpClient server)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user != null)
            {
                user.IsAdmin = true; 
                await _userRepository.SaveChangesAsync();

                // Відправлення повідомлення про успішне надання прав адміністратора
                string responseMessage = "<admin_rights_granted>";
                byte[] responseData = Encoding.Unicode.GetBytes(responseMessage);
                await server.SendAsync(responseData, responseData.Length, clientEndPoint);
            }
        }
        public static async Task RemoveAdminRights(int userId, IPEndPoint clientEndPoint, UdpClient server)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user != null)
            {
                user.IsAdmin = false; 
                await _userRepository.SaveChangesAsync();

                // Відправлення повідомлення про успішне заборонення прав адміністратора
                string resMessage = "<admin_rights_removed>";
                byte[] responseData = Encoding.Unicode.GetBytes(resMessage);
                await server.SendAsync(responseData, responseData.Length, clientEndPoint);
            }
        }
        public static async Task DeleteUser(int userId, IPEndPoint clientEndPoint, UdpClient server)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user != null)
            {
                _userRepository.RemoveUser(user);
                await _userRepository.SaveChangesAsync();

                // Відправлення повідомлення про успішне видалення користувача
                string responsMessage = "<user_deleted>";
                byte[] responseData = Encoding.Unicode.GetBytes(responsMessage);
                await server.SendAsync(responseData, responseData.Length, clientEndPoint);
            }
        }
        public static async Task RemoveMember(IPEndPoint clientEndPoint, UdpClient server)
        {
            byte[] leaveMessage = Encoding.Unicode.GetBytes("<leave>");
            await server.SendAsync(leaveMessage, leaveMessage.Length, clientEndPoint);
        }
        public static async Task SendUserIdResponse(string[] messageParts, IPEndPoint clientEndPoint, UdpClient server)
        {
            try
            {

                if (messageParts.Length >= 2)
                {
                    string email = messageParts[1];
                    var chatUser = await _userRepository.GetUserByEmail(email);

                    // Відправлення відповіді з ID користувача
                    string responseMessage = chatUser.Id.ToString();
                    Console.WriteLine($"ID: {responseMessage}");
                    byte[] responseData = Encoding.Unicode.GetBytes(responseMessage);
                    await server.SendAsync(responseData, responseData.Length, clientEndPoint);
                }
                else
                {
                    Console.WriteLine("Invalid message format for getUserId");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing getUserId message: {ex.Message}");
            }
        }
    }
}

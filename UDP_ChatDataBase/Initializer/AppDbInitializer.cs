using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using Microsoft.EntityFrameworkCore;
using UDP_ChatDataBase.Entity;

namespace UDP_ChatDataBase.Initializer
{
    public static class AppDbInitializer 
    {
        public static void SeedUsers(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().HasData(new Users[]
            {
            new Users() { Id = 1, FullName = "Goch Petro", Email = "goch@gmail.com", Password = "Goch2024", IsAdmin = true},
            });
        }

        public static void SeedChatMessages(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>().HasData(new ChatMessage[]
            {
            new ChatMessage() { Id = 1, Content = "Hello World", Timestamp = DateTime.Now, UsersId = 1},
            new ChatMessage() { Id = 2, Content = "Hello ", Timestamp = DateTime.Now, UsersId = 1},
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using UDP_ChatDataBase.Entity;
using UDP_ChatDataBase.Initializer;

namespace UDP_ChatDataBase.Context
{
    public class DbContextChat : DbContext
    {
        public DbContextChat() : base() { }
        public DbContextChat(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.SeedUsers();
            modelBuilder.SeedChatMessages(); 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Server=DESKTOP-NOA1L8B;Database=Chat;User Id=DESKTOP-NOA1L8B\\andri;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=true;");
        }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Users> Users { get; set; }

    }
}

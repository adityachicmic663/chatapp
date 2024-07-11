
using backendChatApplcation.Models;

using backendChatApplication.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace backendChatApplication
{
    public class chatDataContext : DbContext
    {
        
        public chatDataContext(DbContextOptions<chatDataContext> options) : base(options)
        {
            
        }

        public DbSet<UserModel> users { get; set; }
        public DbSet<chatRoomModel> ChatRooms { get; set; }
        public DbSet<chatMessageModel> ChatMessages { get; set; }
        public DbSet<userChatRoomModel> UserChatRooms { get; set; }
        public DbSet<ConnectedUser> UserConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>()
                .Property(u => u.otpToken)
                .IsRequired(false)
                .HasDefaultValue(null);

            modelBuilder.Entity<UserModel>()
                .Property(u => u.OtpTokenExpiry)
                .IsRequired(false)
                .HasDefaultValue(null);

            modelBuilder.Entity<UserModel>()
                .Property(u => u.profilePicturePath)
                .IsRequired(false)
               .HasDefaultValue(null);

            modelBuilder.Entity<UserModel>().HasKey(u => u.userId);
            modelBuilder.Entity<chatRoomModel>().HasKey(c => c.chatRoomId);
            
        modelBuilder.Entity<chatMessageModel>().HasKey(m => m.chatMessageId);
            modelBuilder.Entity<userChatRoomModel>().HasKey(ur => new { ur.userId, ur.chatRoomId });

           
            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.UserChatRooms)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.userId);

            modelBuilder.Entity<chatRoomModel>()
                .HasMany(c => c.UserChatRooms)
                .WithOne(ur => ur.ChatRoom)
                .HasForeignKey(ur => ur.chatRoomId);

            modelBuilder.Entity<chatMessageModel>()
                .HasOne(m => m.chatRoom)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.chatRoomId);

            modelBuilder.Entity<chatMessageModel>()
                .HasOne(m => m.sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.senderId);


        }

        public void SeedData()
        {
           
                var adminExists = this.users.Any(x => x.userName == "aditya" && x.role == "admin");
            var password = "Aditya@123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            Console.WriteLine($"Hashed Password: {hashedPassword}");

            if (!adminExists)
                {
                try
                {
                    this.Database.ExecuteSqlRaw(@"
                        INSERT INTO users(userName, role, email, password, phoneNumber, emailConfirmed,address, firstLanguage, age, gender,isOnline)
                        VALUES('aditya', 'admin', 'adityabisht8436@gmail.com', {0}, 97643567, true,'Rudrapur','Hindi', 23, 'Male',false)", hashedPassword);
                }catch(Exception ex)
                {
                    Console.WriteLine($"ExceptionOccured{ex.Message}");
                    throw;
                }
                    
                }
                
           
        }
    }
}

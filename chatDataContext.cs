using backendChatApplication.Models;
using Microsoft.EntityFrameworkCore;
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

        }

        public void SeedData()
        {
           
                var adminExists = this.users.Any(x => x.userName == "aditya" && x.role == "admin");
                if (!adminExists)
                {
                    this.Database.ExecuteSqlRaw(@"
                        INSERT INTO users(userName, role, email, password, phoneNumber, emailConfirmed,address, firstLanguage, age, gender)
                        VALUES('aditya', 'admin', 'adityabisht8436@gmail.com', 'Aditya@123', 97643567, true,'Rudrapur','Hindi', 23, 'Male')");
                }
                
           
        }
    }
}

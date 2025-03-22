using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoadDefectsDetection.Server.Models;

namespace RoadDefectsDetection.Server.Data
{
    public class DataContext : IdentityDbContext<UserEntity>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<Pothole> Potholes { get; set; } // Add this line

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Admin ve User rolleri
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
            );

            builder.Entity<Pothole>()
            .HasOne(d => d.User)
            .WithMany() // Kullanıcının birden fazla çukur kaydı olabilir.
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            // Admin kullanıcısı oluşturma
            var adminUser = new UserEntity
            {
                Id = "1",
                UserName = "admin1@example.com",
                NormalizedUserName = "ADMIN1@EXAMPLE.COM",
                Email = "admin1@example.com",
                NormalizedEmail = "ADMIN1@EXAMPLE.COM",
                FullName = "Admin",
                EmailConfirmed = true
            };

            // Admin şifresi
            var passwordHasher = new PasswordHasher<UserEntity>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "adminpassword");

            builder.Entity<UserEntity>().HasData(adminUser);

            // Admin rol ataması
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "1", RoleId = "1" }
            );
        }
    }
}

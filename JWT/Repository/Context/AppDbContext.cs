using JWT.Core;
using JWT.Core.Model;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWT.Repository.Context
{
    public class AppDbContext : IdentityDbContext<CustomUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price)
                      .HasPrecision(18, 2); // 18 basamaklı, 2 ondalık basamak
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

using Foruscorp.Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Auth.DataBase
{
    public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
    {

        public DbSet<User> Users { get; set; }
        //public DbSet<Role> Roles { get; set; } = null!;
        //public DbSet<UserRole> UserRoles { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("User");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserContext).Assembly);
        }
    }
}

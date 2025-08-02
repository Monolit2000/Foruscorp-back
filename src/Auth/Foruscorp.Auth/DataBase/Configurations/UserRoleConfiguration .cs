using Foruscorp.Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Auth.DataBase.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(ur => ur.Id);

            builder.Property(ur => ur.Id).IsRequired();

            builder.Property(ur => ur.Role)
                   .HasConversion<string>() // enum to string
                   .IsRequired();

            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.Roles)
                   .HasForeignKey(ur => ur.UserId);
        }
    }
}

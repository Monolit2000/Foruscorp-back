using Foruscorp.Trucks.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.UserId);

            builder.Property(u => u.UserId)
                   .ValueGeneratedNever()
                   .IsRequired();

            builder.Property(u => u.CreatedAt)
                   .IsRequired();

            builder.Property(u => u.ContactId)
                .IsRequired(false);

            builder.HasOne(u => u.Contact)
                   .WithOne() 
                   .HasForeignKey<User>(u => u.ContactId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

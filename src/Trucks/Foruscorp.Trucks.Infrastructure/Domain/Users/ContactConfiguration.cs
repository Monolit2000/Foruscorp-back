using Foruscorp.Trucks.Domain.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Domain.Users
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.ToTable("Contacts");

            builder.HasKey(c => c.Id); 

            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            builder.Property(c => c.FullName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Phone)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            builder.Property(c => c.TelegramLink)
                .HasMaxLength(200);
        }
    }
}

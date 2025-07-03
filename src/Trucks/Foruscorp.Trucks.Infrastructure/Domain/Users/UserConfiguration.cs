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
                   //.HasColumnType("timestamp without time zone");

            // если хотите, чтобы СУБД по-умолчанию ставила текущее время (необязательно,
            // ведь вы уже устанавливаете CreatedAt в конструкторе):
            // builder.Property(u => u.CreatedAt)
            //        .HasDefaultValueSql("CURRENT_TIMESTAMP")
            //        .ValueGeneratedOnAdd();
        }
    }
}

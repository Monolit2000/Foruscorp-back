using Foruscorp.Push.Domain.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Push.Infrastructure.Domain
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("Devices");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                   .ValueGeneratedNever();

            builder.Property(d => d.UserId)
                   .IsRequired(false);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.OwnsOne(x => x.Token, token =>
            {
                token.Property<string>("Value")
                     .HasColumnName("PushToken")
                     .IsRequired()
                     .HasMaxLength(255);
            });

            builder.Property(d => d.RegisteredAt)
                   .IsRequired();
        }
    }

}

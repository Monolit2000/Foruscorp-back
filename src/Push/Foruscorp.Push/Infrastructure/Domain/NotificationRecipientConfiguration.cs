using Foruscorp.Push.Domain.PushNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Push.Infrastructure.Domain
{
    public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
        {
            builder.ToTable("NotificationRecipients");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .ValueGeneratedNever();

            builder.Property(r => r.NotificationId)
                   .IsRequired();

            builder.Property(r => r.DeviceId)
                   .IsRequired();

            builder.Property<RecipientStatus>("Status")
                   .HasConversion<string>()
                   .HasColumnName("Status")
                   .IsRequired();

            builder.Property(r => r.DeliveredAt)
                   .IsRequired(false);

            builder.Property(r => r.FailureReason)
                   .HasMaxLength(1000)
                   .IsRequired(false);

            // Навигация к Device
            builder.HasOne(r => r.Device)
                   .WithMany()
                   .HasForeignKey(r => r.DeviceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

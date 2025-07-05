using Foruscorp.Push.Domain.PushNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Foruscorp.Push.Infrastructure.Domain
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            // PK
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id)
                   .ValueGeneratedNever();

            // Дата создания
            builder.Property(n => n.CreatedAt)
                   .IsRequired();

            // Статус как строка
            builder.Property(n => n.Status)
                   .HasConversion<string>()
                   .HasColumnName("Status")
                   .IsRequired();

            // ValueObject: Content (Title, Body)
            builder.OwnsOne(n => n.Content, content =>
            {
                content.Property(c => c.Title)
                       .HasColumnName("Title")
                       .IsRequired()
                       .HasMaxLength(200);

                content.Property(c => c.Body)
                       .HasColumnName("Body")
                       .IsRequired()
                       .HasMaxLength(1000);
            });

            // ValueObject: Payload.Data хранится в JSONB
            builder.OwnsOne(n => n.Payload, payload =>
            {
                payload.Property(p => p.Data)
                       .HasColumnName("Payload")
                       .HasConversion(
                           // Явно приводим null к JsonSerializerOptions, чтобы выбрать нужнюю перегрузку
                           d => JsonSerializer.Serialize(d, (JsonSerializerOptions)null),
                           s => JsonSerializer.Deserialize<Dictionary<string, object>>(s, (JsonSerializerOptions)null)
                       )
                       .HasColumnType("jsonb")   
                       .IsRequired();
            });

            // One-to-many: Notification → Recipients
            builder.HasMany(n => n.Recipients)
                   .WithOne(r => r.Notification)
                   .HasForeignKey(r => r.NotificationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

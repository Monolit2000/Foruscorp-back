using Foruscorp.Push.Domain.Devices;
using Foruscorp.Push.Domain.PushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Infrastructure.Database
{
    public class PushNotificationsContext(DbContextOptions<PushNotificationsContext> options) : DbContext(options)
    {
        public DbSet<Device> Devices { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("PushNotifications");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PushNotificationsContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}

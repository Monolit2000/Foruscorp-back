using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Trucks
{
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            // Map to the "Routes" table in the "TuckTracking" schema
            builder.ToTable("Routes", "TuckTracking");

            // Define the primary key
            builder.HasKey(r => r.Id);

            // Configure properties
            builder.Property(r => r.RouteId)
                .HasColumnName("RouteId")
                .IsRequired();

            builder.Property(r => r.TruckTrackerId)
                .HasColumnName("TruckTrackerId")
                .IsRequired();

            builder.Property(r => r.TruckId)
                .HasColumnName("TruckId")
                .IsRequired();

            // Configure relationship with TruckTracker
            builder.HasOne<TruckTracker>()
                .WithMany(t => t.Routes)
                .HasForeignKey(r => r.TruckTrackerId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove routes when the associated TruckTracker is deleted

            //builder.HasMany(r => r.TruckLocations)
            //    .WithOne()
            //    .HasForeignKey(tl => tl.RouteId)
            //    .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove locations when the route is deleted  

            // Optional: Index for performance
            builder.HasIndex(r => r.TruckTrackerId);

            builder.HasIndex(r => r.TruckId);
        }
    }
}
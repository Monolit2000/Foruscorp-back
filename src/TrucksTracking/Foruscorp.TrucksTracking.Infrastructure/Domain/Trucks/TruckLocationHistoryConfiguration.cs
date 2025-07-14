using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Trucks
{
    public class TruckLocationHistoryConfiguration : IEntityTypeConfiguration<TruckLocation>
    {
        public void Configure(EntityTypeBuilder<TruckLocation> builder)
        {
            // Table mapping
            builder.ToTable("TruckLocationHistory", schema: "TuckTracking");

            // Primary Key
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id)
                   .HasColumnName("TruckLocationId")
                   .ValueGeneratedOnAdd();

            builder.Property(h => h.RouteId)
                   .IsRequired(false);

            //builder.HasOne<Route>()
            //       .WithMany(r => r.TruckLocations)
            //       .HasForeignKey(h => h.RouteId)
            //       .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<TruckTracker>()
                .WithOne(x => x.CurrentTruckLocation)
                .HasForeignKey(x => x.CurrentTruckLocationId);

            // Foreign Key to TruckTracker

            builder.Property(h => h.TruckId)
                   .IsRequired();
            builder.HasOne<TruckTracker>()
                   .WithMany(t => t.TruckLocationHistory)
                   .HasForeignKey(h => h.TruckTrackerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(l => l.FormattedLocation)
             .IsRequired(false);

            // GeoPoint value object mapping
            builder.OwnsOne(h => h.Location, geoPointBuilder =>
            {
                geoPointBuilder.Property(gp => gp.Latitude)
                               .HasColumnName(nameof(GeoPoint.Latitude))
                               .HasColumnType("decimal(9,6)")
                               .IsRequired();

                geoPointBuilder.Property(gp => gp.Longitude)
                               .HasColumnName(nameof(GeoPoint.Longitude))
                               .HasColumnType("decimal(9,6)")
                               .IsRequired();
            });

            // RecordedAt
            builder.Property(h => h.RecordedAt)
                   .IsRequired();

        }
    }
}

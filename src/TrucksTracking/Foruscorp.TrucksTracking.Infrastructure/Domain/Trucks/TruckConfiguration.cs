using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Trucks
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder.ToTable("Trucks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("TruckId")
                .ValueGeneratedOnAdd();

            builder.Property(t => t.TruckId)
                .IsRequired();

            builder.Property(t => t.CurrentRouteId)
                .IsRequired();

            builder.Property(t => t.FuelStatus)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.OwnsOne(t => t.CurrentTruckLocation, locationBuilder =>
            {
                locationBuilder.ToTable("CurrentTruckLocations");

                locationBuilder.Property(l => l.Id)
                    .HasColumnName("TruckLocationId")
                    .ValueGeneratedOnAdd();

                locationBuilder.Property(l => l.TruckId)
                    .IsRequired();

                locationBuilder.OwnsOne(l => l.Location, geoPointBuilder =>
                {
                    geoPointBuilder.Property(gp => gp.Latitude)
                        .HasColumnName("Latitude")
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");

                    geoPointBuilder.Property(gp => gp.Longitude)
                        .HasColumnName("Longitude")
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");
                });

                locationBuilder.Property(l => l.RecordedAt)
                    .IsRequired()
                    .HasColumnType("datetime");
            });

            builder.OwnsMany(t => t.TruckLocationHistory, historyBuilder =>
            {
                historyBuilder.ToTable("TruckLocationHistory");

                historyBuilder.WithOwner()
                    .HasForeignKey(x => x.TruckId);

                historyBuilder.Property(h => h.Id)
                    .HasColumnName("TruckLocationId")
                    .ValueGeneratedOnAdd();

                historyBuilder.Property(h => h.TruckId)
                    .IsRequired();

                historyBuilder.OwnsOne(h => h.Location, geoPointBuilder =>
                {
                    geoPointBuilder.Property(gp => gp.Latitude)
                        .HasColumnName(nameof(GeoPoint.Latitude))
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");

                    geoPointBuilder.Property(gp => gp.Longitude)
                        .HasColumnName(nameof(GeoPoint.Longitude))
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");
                });

                historyBuilder.Property(h => h.RecordedAt)
                    .IsRequired()
                    .HasColumnType("datetime");
            });

            builder.OwnsMany(t => t.FuelHistory, fuelBuilder =>
            {
                fuelBuilder.ToTable("TruckFuelHistory");

                fuelBuilder.WithOwner().HasForeignKey(x => x.TruckId);

                fuelBuilder.Property(f => f.Id).HasColumnName("TruckFuelId").ValueGeneratedOnAdd();

                fuelBuilder.Property(f => f.TruckId).IsRequired();

                fuelBuilder.Property(f => f.PreviousFuelLevel).IsRequired().HasColumnType("decimal(18,2)");

                fuelBuilder.Property(f => f.NewFuelLevel).IsRequired().HasColumnType("decimal(18,2)");

                fuelBuilder.OwnsOne(f => f.Location, geoPointBuilder =>
                {
                    geoPointBuilder.Property(gp => gp.Latitude)
                        .HasColumnName(nameof(GeoPoint.Latitude))
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");

                    geoPointBuilder.Property(gp => gp.Longitude)
                        .HasColumnName(nameof(GeoPoint.Longitude))
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");
                });

                fuelBuilder.Property(f => f.RecordedAt) .IsRequired() .HasColumnType("datetime");
            });

            builder.HasIndex(t => t.TruckId);
            builder.HasIndex(t => t.CurrentRouteId);
        }
    }
}
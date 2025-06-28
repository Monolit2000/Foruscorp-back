using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Trucks
{
    public class FuelHistoryConfiguration : IEntityTypeConfiguration<TruckFuel>
    {
        public void Configure(EntityTypeBuilder<TruckFuel> builder)
        {
            // Map to separate table
            builder.ToTable("TruckFuelHistory", schema: "TuckTracking");

            // Primary Key
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id)
                   .HasColumnName("TruckFuelId")
                   .ValueGeneratedOnAdd();

            // Foreign Key to TruckTracker
            builder.Property(f => f.TruckId)
                   .IsRequired();
            builder.HasOne<TruckTracker>()
                   .WithMany(t => t.FuelHistory)
                   .HasForeignKey(f => f.TruckId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Fuel level properties
            builder.Property(f => f.PreviousFuelLevel)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            builder.Property(f => f.NewFuelLevel)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            // GeoPoint value object mapping
            builder.OwnsOne(f => f.Location, geoPointBuilder =>
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

            // Recorded timestamp
            builder.Property(f => f.RecordedAt)
                   .IsRequired();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Trucks
{
    public class TruckConfiguration : IEntityTypeConfiguration<TruckTracker>
    {
        public void Configure(EntityTypeBuilder<TruckTracker> builder)
        {
            builder.ToTable("TruckTrackers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
            .HasColumnName("Id");

            builder.Property(t => t.TruckId)
                .HasColumnName("TruckId")
                .IsRequired();

            builder.Property(t => t.FuelStatus)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.TruckStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.TruckTrackerStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne(t => t.CurrentTruckLocation)
                   .WithOne()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TruckLocationHistory)
                   .WithOne()
                   .HasForeignKey(h => h.TruckTrackerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.FuelHistory)
                   .WithOne()
                   .HasForeignKey(h => h.TruckTrackerId)
                   .OnDelete(DeleteBehavior.Cascade);

            //builder.Property(t => t.CurrentRouteId)
            //    .IsRequired(false); // Since CurrentRouteId is Guid? (nullable)

            builder.HasOne(t => t.CurrentRoute)
                .WithOne()
                //.HasForeignKey<TruckTracker>(tt => tt.CurrentRouteId)
                .OnDelete(DeleteBehavior.SetNull);


            builder.HasIndex(t => t.TruckId);
            //builder.HasIndex(t => t.CurrentRouteId);

            //builder.HasIndex(t => t.CurrentRouteId)
            //    .IsUnique(false);
            //.HasDatabaseName("IX_TruckTrackers_CurrentRouteId");
        }
    }
}
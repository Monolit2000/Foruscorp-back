using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.FuelRoutes.Infrastructure.Data.Configurations
{
    public class FuelRouteStationConfiguration : IEntityTypeConfiguration<FuelRouteStation>
    {
        public void Configure(EntityTypeBuilder<FuelRouteStation> builder)
        {
            builder.ToTable("RouteFuelPoints");

            builder.HasKey(x => x.FuelStationId);

            builder.Property(x => x.FuelStationId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.FuelRouteId)
                .IsRequired();

            builder.Property(x => x.FuelPointId)
                .IsRequired();

            builder.Property(x => x.ScheduledTime)
                .IsRequired();

            builder.Property(x => x.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Discount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PriceAfterDiscount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Latitude)
                .IsRequired(false);

            builder.Property(x => x.Longitude)
                .IsRequired(false);

            builder.Property(x => x.Name)
                .IsRequired(false);

            builder.Property(x => x.Address)
                .IsRequired(false);

            builder.Property(x => x.IsAlgorithm)
                .IsRequired();

            builder.Property(x => x.Refill)
                .IsRequired(false);

            builder.Property(x => x.StopOrder)
                .IsRequired();

            builder.Property(x => x.NextDistanceKm)
                .IsRequired(false);

            builder.Property(x => x.RoadSectionId)
                .IsRequired();

            builder.Property(x => x.ForwardDistance)
                .IsRequired()
                .HasDefaultValue(0.0)
                .HasColumnType("double precision");

            builder.Property(x => x.IsPlaned)
                .IsRequired();

            builder.Property(x => x.IsAssigned)
                .IsRequired();


            // Внешний ключ на FuelRoute
            builder.HasOne<FuelRoute>()
                   .WithMany(fr => fr.FuelRouteStations)
                   .HasForeignKey(x => x.FuelRouteId);


            builder.HasOne<FuelRouteSection>()
                  .WithMany(fr => fr.FuelRouteStations)
                  .HasForeignKey(x => x.RoadSectionId);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.FuelRoutes.Infrastructure.Data.Configurations
{
    public class FuelRouteConfiguration : IEntityTypeConfiguration<FuelRoute>
    {
        public void Configure(EntityTypeBuilder<FuelRoute> builder)
        {
            builder.ToTable("FuelRoutes");

            builder.HasKey(fr => fr.Id);

            builder.Property(fr => fr.Id)
                .HasColumnName("FuelRouteId")
                .ValueGeneratedOnAdd();

            //builder.Property(fr => fr.DriverId)
            //    .IsRequired();

            builder.Property(fr => fr.TruckId)
                .IsRequired();

            builder.Property(fr => fr.Origin)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(fr => fr.Destination)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(fr => fr.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            builder.Property(fr => fr.ChangedAt)
                .IsRequired()
                .HasColumnType("datetime");

            builder.OwnsMany(fr => fr.FuelPoints, fuelPointBuilder =>
            {
                fuelPointBuilder.ToTable("RouteFuelPoints");

                fuelPointBuilder.HasKey(fp => fp.FuelPointId);

                fuelPointBuilder.Property(fp => fp.FuelPointId)
                    .HasColumnName("FuelPointId")
                    .ValueGeneratedOnAdd();

                fuelPointBuilder.Property(fp => fp.FuelRouteId)
                    .IsRequired();

                fuelPointBuilder.OwnsOne(fp => fp.Location, geoPointBuilder =>
                {
                    geoPointBuilder.Property(gp => gp.Latitude)
                        .HasColumnName(nameof(GeoPoint.Latitude))
                        .IsRequired()
                        .HasColumnType("decimal(9,6)"); 

                    geoPointBuilder.Property(gp => gp.Longitude)
                        .HasColumnName(nameof(GeoPoint))
                        .IsRequired()
                        .HasColumnType("decimal(9,6)");
                });


                fuelPointBuilder.Property(fp => fp.FuelPrice)
                    .HasColumnName(nameof(RouteFuelPoint.FuelPrice))
                    .IsRequired()
                    .HasColumnType("decimal(18,2)"); 

                fuelPointBuilder.Property(fp => fp.ScheduledTime)
                    .IsRequired()
                    .HasColumnType("datetime");

                fuelPointBuilder.WithOwner()
                    .HasForeignKey(fp => fp.FuelRouteId);
            });

            //builder.HasIndex(fr => fr.DriverId);
            builder.HasIndex(fr => fr.TruckId);
        }
    }
}
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Runtime.CompilerServices;

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

            builder.Property(fr => fr.TruckId)
                .IsRequired();

            builder.Property(fr => fr.EncodeRoute)
                .IsRequired(false);

            builder.Property(fr => fr.CreatedAt)
                .IsRequired();

            builder.Property(fr => fr.ChangedAt)
                .IsRequired();


            builder.OwnsMany(fr => fr.RouteSections, routeSectionBuilder =>
            {
                routeSectionBuilder.HasKey(fp => fp.Id);

                routeSectionBuilder.ToTable("RouteSections");

                routeSectionBuilder.WithOwner().HasForeignKey(rs => rs.RouteId);

                routeSectionBuilder.Property(rs => rs.EncodeRoute).IsRequired(true);    
            });


            builder.OwnsMany(fr => fr.FuelStopStations, fuelPointBuilder =>
            {
                fuelPointBuilder.ToTable("RouteFuelPoints");

                fuelPointBuilder.WithOwner().HasForeignKey("FuelRouteId");

                fuelPointBuilder.HasKey(fp => fp.FuelPointId);

                fuelPointBuilder.Property(fp => fp.FuelPointId)
                    .HasColumnName("FuelPointId")
                    .ValueGeneratedOnAdd();

                fuelPointBuilder.Property(fp => fp.FuelRouteId)
                    .IsRequired();

                fuelPointBuilder.Property(fp => fp.SectionId)
                    .IsRequired(false);

                fuelPointBuilder.OwnsOne(fp => fp.Location, geoPointBuilder =>
                {
                    geoPointBuilder.Property(gp => gp.Latitude)
                        .HasColumnName(nameof(GeoPoint.Latitude))
                        .IsRequired(); 

                    geoPointBuilder.Property(gp => gp.Longitude)
                        .HasColumnName(nameof(GeoPoint))
                        .IsRequired();
                });


                fuelPointBuilder.Property(fp => fp.FuelPrice)
                    .HasColumnName(nameof(FuelStopStation.FuelPrice))
                    .IsRequired()
                    .HasColumnType("decimal(18,2)"); 

                fuelPointBuilder.Property(fp => fp.ScheduledTime)
                    .IsRequired();

                fuelPointBuilder.WithOwner()
                    .HasForeignKey(fp => fp.FuelRouteId);
            });

            builder.HasMany(fr => fr.MapPoints)
                .WithOne()
                .HasForeignKey(mp => mp.RouteId);

            //builder.OwnsMany(fr => fr.MapPoints, mp =>
            //{
            //    mp.ToTable("MapPoints");

            //    mp.HasKey(p => p.RoadSectionId);

            //    mp.Property(p => p.RouteSectionId)
            //        .IsRequired();

            //    mp.OwnsOne(p => p.GeoPoint, gp =>
            //    {
            //        gp.Property(g => g.Latitude)
            //            .HasColumnName("Latitude")
            //            .IsRequired();

            //        gp.Property(g => g.Longitude)
            //            .HasColumnName("Longitude")
            //            .IsRequired();
            //    });
            //});

            //builder.HasIndex(fr => fr.DriverId);
            builder.HasIndex(fr => fr.TruckId);
        }
    }
}
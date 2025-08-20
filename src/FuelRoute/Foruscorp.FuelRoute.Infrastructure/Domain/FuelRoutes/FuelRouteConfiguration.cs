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


            builder.Property(fr => fr.CreatedAt)
                .IsRequired();

            builder.Property(fr => fr.ChangedAt)
                .IsRequired();

            builder.Property(fr => fr.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            builder.Property(fr => fr.RemainingFuel)
                .IsRequired();

            builder.Property(fr => fr.IsComplet)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(fr => fr.IsDeclined)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(fr => fr.DeclinedAt)
                .IsRequired(false);


            //builder.OwnsMany(fr => fr.RouteSections, routeSectionBuilder =>
            //{
            //    routeSectionBuilder.HasKey(fp => fp.Id);

            //    routeSectionBuilder.ToTable("RouteSections");

            //    routeSectionBuilder.WithOwner().HasForeignKey(rs => rs.RouteId);

            //    routeSectionBuilder.Property(rs => rs.EncodeRoute).IsRequired(true);    

                
            //});


            //builder.OwnsMany(fr => fr.FuelRouteStations, fuelPointBuilder =>
            //{
            //    fuelPointBuilder.ToTable("RouteFuelPoints");

            //    fuelPointBuilder.WithOwner().HasForeignKey(x => x.FuelRouteId);

            //    //fuelPointBuilder.HasKey(fp => fp.FuelPointId);
            //    fuelPointBuilder.HasKey(fp => fp.FuelStationId);



            //    fuelPointBuilder.Property(fp => fp.FuelPointId)
            //        .HasColumnName("FuelPointId");
            //        //.ValueGeneratedOnAdd();

            //    fuelPointBuilder.Property(fp => fp.FuelRouteId)
            //        .IsRequired();

            //    fuelPointBuilder.Property(fp => fp.Price)
            //        .HasColumnName(nameof(FuelRouteStations.Price))
            //        .IsRequired()
            //        .HasColumnType("decimal(18,2)");

            //    fuelPointBuilder.Property(fp => fp.Discount)
            //        .HasColumnName(nameof(FuelRouteStations.Discount))
            //        .IsRequired()
            //        .HasColumnType("decimal(18,2)");

            //    fuelPointBuilder.Property(fp => fp.PriceAfterDiscount)
            //        .HasColumnName(nameof(FuelRouteStations.PriceAfterDiscount))
            //        .IsRequired()
            //        .HasColumnType("decimal(18,2)");

            //    fuelPointBuilder.Property(fp => fp.ScheduledTime)
            //        .IsRequired();

            //    fuelPointBuilder.Property(fp => fp.Latitude)
            //        .IsRequired();

            //    fuelPointBuilder.Property(fp => fp.Longitude)
            //        .IsRequired(false);

            //    fuelPointBuilder.Property(fp => fp.Name)
            //        .IsRequired(false);

            //    fuelPointBuilder.Property(fp => fp.Address)
            //        .IsRequired(false);

            //    fuelPointBuilder.Property(fp => fp.IsAlgorithm)
            //        .IsRequired();

            //    fuelPointBuilder.Property(fp => fp.Refill)
            //        .IsRequired(false);

            //    fuelPointBuilder.Property(fp => fp.StopOrder)
            //        .IsRequired();

            //    fuelPointBuilder.Property(fp => fp.NextDistanceKm)
            //        .IsRequired(false);

            //    fuelPointBuilder.Property(fp => fp.RoadSectionId)
            //        .IsRequired(false);
            //});

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
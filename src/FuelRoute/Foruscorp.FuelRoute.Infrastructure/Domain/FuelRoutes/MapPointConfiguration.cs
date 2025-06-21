using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Infrastructure.Data.Configurations
{
    public class MapPointConfiguration : IEntityTypeConfiguration<MapPoint>
    {
        public void Configure(EntityTypeBuilder<MapPoint> builder)
        {
            builder.ToTable("MapPoints");

            builder.HasKey(mp => mp.Id);

            //builder.Property(mp => mp.Id)
            //    .ValueGeneratedOnAdd();

            builder.Property(mp => mp.RouteId)
                .IsRequired();

            //builder.HasIndex(mp => mp.RouteSectionId); // для прискорення JOIN-ів по маршруту

            builder.OwnsOne(mp => mp.GeoPoint, gp =>
            {
                gp.Property(p => p.Latitude)
                    .HasColumnName("Latitude")
                    .IsRequired();

                gp.Property(p => p.Longitude)
                    .HasColumnName("Longitude")
                    .IsRequired();
            });

            // Якщо є навігаційна властивість до FuelRoute:
            // builder.HasOne<FuelRoute>()
            //        .WithMany(fr => fr.MapPoints)
            //        .HasForeignKey(mp => mp.RouteSectionId);
        }
    }
}

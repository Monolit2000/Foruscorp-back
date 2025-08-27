using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.FuelRoutes.Infrastructure.Data.Configurations
{
    public class LocationPointConfiguration : IEntityTypeConfiguration<LocationPoint>
    {
        public void Configure(EntityTypeBuilder<LocationPoint> builder)
        {
            builder.ToTable("LocationPoints");

            builder.HasKey(lp => lp.Id);

            builder.Property(lp => lp.Id)
                .HasColumnName("LocationPointId")
                .ValueGeneratedNever(); 

            builder.Property(lp => lp.RouteId)
                .IsRequired();

            builder.Property(lp => lp.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(lp => lp.Latitude)
                .IsRequired()
                .HasColumnType("double precision");

            builder.Property(lp => lp.Longitude)
                .IsRequired()
                .HasColumnType("double precision");

            builder.Property(lp => lp.RouteVersion)
                .IsRequired();

            builder.Property(lp => lp.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(lp => lp.RouteId)
                .IsRequired(false);

            builder.Property(lp => lp.FuelRouteSectionId)
                .IsRequired(false);

            builder.HasOne<FuelRoute>()
                .WithMany()
                .HasForeignKey(lp => lp.RouteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<FuelRouteSection>()
                .WithMany(frs => frs.LocationPoints)
                .HasForeignKey(lp => lp.FuelRouteSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Foruscorp.FuelRoutes.Domain.RouteValidators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.FuelRoutes.Infrastructure.Domain.RouteValidators
{
    public class FuelStationChangeConfiguration : IEntityTypeConfiguration<FuelStationChange>
    {
        public void Configure(EntityTypeBuilder<FuelStationChange> builder)
        {

            builder.HasKey(fsc => fsc.FuelStationChangeId);

            builder.Property(fsc => fsc.FuelStationChangeId)
                .ValueGeneratedOnAdd();

            builder.Property(fsc => fsc.Id);

            builder.Property(fsc => fsc.RouteValidatorId)
                .IsRequired();

            builder.Property(fsc => fsc.FuelRouteStationId)
                .IsRequired();

            builder.Property(fsc => fsc.ForwardDistance)
                .IsRequired()
                .HasColumnType("double precision");

            builder.Property(fsc => fsc.NextDistanceKm)
                .IsRequired()
                .HasColumnType("double precision");

            builder.Property(fsc => fsc.StopOrder)
                .IsRequired();

            builder.Property(fsc => fsc.Refill)
                .IsRequired()
                .HasColumnType("double precision");

            builder.Property(fsc => fsc.CurrentFuel)
                .IsRequired()
                .HasColumnType("double precision");

            builder.Property(fsc => fsc.IsAlgo)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(fsc => fsc.IsManual)
                .IsRequired()
                .HasDefaultValue(false);

            // (Many-to-One)
            builder.HasOne(fsc => fsc.FuelStation)
                .WithMany()
                .HasForeignKey(fsc => fsc.FuelRouteStationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(fsc => fsc.RouteValidatorId);
            builder.HasIndex(fsc => fsc.FuelRouteStationId);
            builder.HasIndex(fsc => fsc.IsAlgo);
            builder.HasIndex(fsc => fsc.IsManual);

            //builder.HasIndex(fsc => new { fsc.RouteValidatorId, fsc.IsAlgo });
        }
    }
}

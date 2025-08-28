using Foruscorp.FuelRoutes.Domain.RouteValidators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.FuelRoutes.Infrastructure.Domain.RouteValidators
{
    public class RouteValidatorConfiguration : IEntityTypeConfiguration<RouteValidator>
    {
        public void Configure(EntityTypeBuilder<RouteValidator> builder)
        {
            builder.HasKey(rv => rv.Id);

            builder.Property(rv => rv.Id)
                .ValueGeneratedNever();

            builder.Property(rv => rv.IsValid)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne(rv => rv.FuelRoute)
                .WithMany()
                .HasForeignKey("FuelRouteId")
                .OnDelete(DeleteBehavior.Cascade);

            // (One-to-One)
            builder.HasOne(rv => rv.FuelRouteSection)
                .WithMany()
                .HasForeignKey("FuelRouteSectionId")
                .OnDelete(DeleteBehavior.Cascade);

            // (One-to-Many)
            builder.HasMany(rv => rv.FuelStationChanges)
                .WithOne()
                .HasForeignKey(fsc => fsc.RouteValidatorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex("FuelRouteId");
            builder.HasIndex("FuelRouteSectionId");
            builder.HasIndex(rv => rv.IsValid);
        }
    }
}

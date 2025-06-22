using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FuelRouteSectionConfiguration : IEntityTypeConfiguration<FuelRouteSection>
{
    public void Configure(EntityTypeBuilder<FuelRouteSection> builder)
    {
        builder.ToTable("RouteSections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EncodeRoute)
               .IsRequired();

        builder.Property(x => x.RouteId)
               .IsRequired();

        builder.HasOne<FuelRoute>()
               .WithMany(x => x.RouteSections)
               .HasForeignKey(x => x.RouteId);
    }
}

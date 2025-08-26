using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net.NetworkInformation;

public class FuelRouteSectionConfiguration : IEntityTypeConfiguration<FuelRouteSection>
{
    public void Configure(EntityTypeBuilder<FuelRouteSection> builder)
    {
        builder.ToTable("RouteSections");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
             .ValueGeneratedNever();

        builder.Property(x => x.EncodeRoute)
               .IsRequired();

        builder.Property(x => x.RouteId)
               .IsRequired();

        builder.Property(x => x.IsAssigned)
            .IsRequired();

        builder.Property(fs => fs.FuelNeeded)
            .IsRequired();

        builder.Property(fr => fr.IsEdited)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(fr => fr.IsAccepted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.OwnsOne(x => x.RouteSectionInfo, info =>
        {
            info.Property(p => p.Tolls)
                .HasColumnName(nameof(RouteSectionInfo.Tolls))
                .IsRequired();

            info.Property(p => p.Gallons)
                .HasColumnName(nameof(RouteSectionInfo.Gallons))
                .IsRequired();

            info.Property(p => p.Miles)
                .HasColumnName(nameof(RouteSectionInfo.Miles))
                .IsRequired();

            info.Property(p => p.DriveTime)
                .HasColumnName(nameof(RouteSectionInfo.DriveTime))
                .IsRequired();
        });

        builder.HasOne<FuelRoute>()
               .WithMany(x => x.RouteSections)
               .HasForeignKey(x => x.RouteId);
    }
}

using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.RouteOffers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Domain.RouteOffers
{
    public class RouteOfferConfiguration : IEntityTypeConfiguration<RouteOffer>
    {
        public void Configure(EntityTypeBuilder<RouteOffer> builder)
        {
            builder.ToTable("RouteOffers");

            builder.HasKey(ro => ro.Id);

            builder.Property(ro => ro.Id)
                .HasColumnName("RouteOfferId")
                .ValueGeneratedOnAdd();

            builder.Property(ro => ro.DriverId)
                .IsRequired();

            builder.Property(ro => ro.Description)
                .IsRequired();

            builder.Property(ro => ro.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(ro => ro.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(ro => ro.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Driver) 
                .WithMany()
                .HasForeignKey(ro => ro.DriverId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Index
            builder.HasIndex(ro => ro.DriverId);
        }
    }
}

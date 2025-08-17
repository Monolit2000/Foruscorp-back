using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Domain.FuelStations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.FuelStations.Infrastructure.Domain.FuelStations
{
    public class FuelStationConfiguration : IEntityTypeConfiguration<FuelStation>
    {
        public void Configure(EntityTypeBuilder<FuelStation> builder)
        {
            builder.ToTable("FuelStations");

            builder.HasKey(fs => fs.Id);

            builder.Property(fs => fs.Id)
                .HasColumnName("FuelStationId")
                .ValueGeneratedOnAdd();


            builder.Property(fs => fs.FuelStationProviderId)
                .IsRequired(false);

            builder.HasIndex(fs => fs.FuelStationProviderId);


            builder.Property(fs => fs.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(fs => fs.FuelProvider)
                .HasMaxLength(100);

            builder.Property(fs => fs.LastUpdated)
                .IsRequired();


            builder.Property(fs => fs.ProviderName)
                .IsRequired(false);

            builder.OwnsOne(fs => fs.Coordinates, coordBuilder =>
            {
                coordBuilder.Property(c => c.Latitude)
                    .HasColumnName("Latitude")
                    .IsRequired();

                coordBuilder.Property(c => c.Longitude)
                    .HasColumnName("Longitude")
                    .IsRequired();

                coordBuilder.HasIndex(c => c.Latitude);

                coordBuilder.HasIndex(c => c.Longitude);
            });

            builder.OwnsMany(fs => fs.FuelPrices, priceBuilder =>
            {
                priceBuilder.ToTable("FuelPrices");

                priceBuilder.WithOwner()
                    .HasForeignKey("FuelStationId");

                priceBuilder.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                priceBuilder.HasKey("Id");

                priceBuilder.Property(fp => fp.FuelType)
                    .IsRequired()
                    .HasConversion(
                        ft => ft.Name,
                        name => new FuelType(name))
                    .HasMaxLength(50);

                priceBuilder.Property(fp => fp.Price)
                    .IsRequired();

                priceBuilder.Property(fp => fp.CreatedAt);

                priceBuilder.Property(fp => fp.UpdatedAt);

                priceBuilder.Property(fp => fp.DiscountedPrice);
            });

            builder.HasIndex(fs => fs.Address);
        }
    }
}

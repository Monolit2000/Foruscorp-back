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

            builder.Property(fs => fs.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(fs => fs.FuelProvider)
                .HasMaxLength(100);

            builder.Property(fs => fs.LastUpdated)
                .IsRequired();
            

            builder.OwnsOne(fs => fs.Coordinates, coordBuilder =>
            {
                coordBuilder.Property(c => c.Latitude)
                    .HasColumnName("Latitude")
                    .IsRequired()
                    .HasColumnType("decimal(9,6)");

                coordBuilder.Property(c => c.Longitude)
                    .HasColumnName("Longitude")
                    .IsRequired()
                    .HasColumnType("decimal(9,6)");
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
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                priceBuilder.Property(fp => fp.DiscountedPrice)
                    .HasColumnType("decimal(18,2)");
            });

            builder.HasIndex(fs => fs.Address);
        }
    }
}

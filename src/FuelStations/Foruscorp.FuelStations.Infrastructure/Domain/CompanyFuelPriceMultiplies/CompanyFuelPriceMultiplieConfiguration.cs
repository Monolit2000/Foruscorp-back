using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.FuelStations.Domain.CompanyFuelPriceMultipliers;

namespace Foruscorp.FuelStations.Infrastructure.Domain.CompanyFuelPriceMultiplies
{
    internal class CompanyFuelPriceMultiplieConfiguration : IEntityTypeConfiguration<CompanyFuelPriceMultiplier>
    {
        public void Configure(EntityTypeBuilder<CompanyFuelPriceMultiplier> builder)
        {
            builder.ToTable("CompanyFuelPriceMultipliers");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.CompanyId)
                .IsRequired();

            builder.Property(e => e.FuelProviderId);

            builder.Property(e => e.PriceMultiplier)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.ChangedAt)
                .IsRequired();

            builder.HasIndex(e => e.CompanyId);

            builder.HasIndex(e => e.FuelProviderId);
        }
    }
}

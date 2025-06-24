using Foruscorp.FuelStations.Domain.FuelMapProvaiders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.FuelStations.Infrastructure.Domain.FuelMapProvaiders
{
    public class FuelMapProvaiderConfiguration : IEntityTypeConfiguration<FuelMapProvaider>
    {
        public void Configure(EntityTypeBuilder<FuelMapProvaider> builder)
        {
            builder.ToTable("FuelMapProvaiders");

            builder.HasKey(fmp => fmp.Id);

            //builder.Property(fmp => fmp.RoadSectionId)
            //    .HasColumnName("FuelMapProvaiderId")
            //    .ValueGeneratedOnAdd();

            builder.Property(fmp => fmp.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(fmp => fmp.Url)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(fmp => fmp.ApiToken)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(fmp => fmp.RefreshedAt)
                .IsRequired();

            builder.HasIndex(fmp => fmp.Name)
                .IsUnique();

            builder.HasIndex(fmp => fmp.Url)
                .IsUnique();
        }
    }
}

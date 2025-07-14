using Foruscorp.TrucksTracking.Worker.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.TrucksTracking.Worker.Infrastructure.DomainConfiguration
{
    public class TruckTrackerConfiguration : IEntityTypeConfiguration<TruckTracker>
    {
        public void Configure(EntityTypeBuilder<TruckTracker> builder)
        {
            builder.HasKey(d => d.TruckId);

            builder.Property(d => d.TruckId)
                   .ValueGeneratedNever();

            builder.Property(d => d.ProviderTruckId)
                .IsRequired(true);
        }
    }
}

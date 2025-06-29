using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Trucks
{
    public class FuelHistoryConfiguration : IEntityTypeConfiguration<TruckFuel>
    {
        public void Configure(EntityTypeBuilder<TruckFuel> builder)
        {
            builder.ToTable("TruckFuelHistory", schema: "TuckTracking");

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id)
                   .HasColumnName("TruckFuelId")
                   .ValueGeneratedOnAdd();

            builder.Property(f => f.TruckId)
                   .IsRequired();
            builder.HasOne<TruckTracker>()
                   .WithMany(t => t.FuelHistory)
                   .HasForeignKey(f => f.TruckId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(f => f.PreviousFuelLevel)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            builder.Property(f => f.NewFuelLevel)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.TruckLocationId)
                   .IsRequired(false); 
            builder.HasOne(f => f.TruckLocation)
                   .WithMany() 
                   .HasForeignKey(f => f.TruckLocationId)
                   .OnDelete(DeleteBehavior.Restrict); 


            builder.Property(f => f.RecordedAt)
                   .IsRequired();
        }
    }
}

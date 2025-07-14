using Foruscorp.TrucksTracking.Domain.FuelStationPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.FuelStationPlans
{
    public class NearFuelStationPlanConfiguration : IEntityTypeConfiguration<NearFuelStationPlan>
    {
        public void Configure(EntityTypeBuilder<NearFuelStationPlan> builder)
        {
            builder.ToTable("NearFuelStationPlans");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.TruckId)
                   .IsRequired();

            builder.Property(x => x.FuelStationId)
                   .IsRequired();

            builder.Property(x => x.Longitude)
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(x => x.Latitude)
                   .HasColumnType("double precision")
                   .IsRequired();

            builder.Property(x => x.NearDistance)
                   .HasColumnType("double precision");

            builder.Property(x => x.IsNear);

            builder.Property(x => x.CreatedAt);

            builder.Property(x => x.RecordedOnLocationId)
                    .IsRequired(false);

            builder.HasOne(x => x.RecordedOnLocation)
                   .WithMany() 
                   .HasForeignKey(x => x.RecordedOnLocationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TruckId);
            builder.HasIndex(x => x.FuelStationId);
            builder.HasIndex(x => x.RecordedOnLocationId);
        }
    }
}

using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Infrastructure.Domain.TruckUsages
{
    public class TruckUsageConfiguration : IEntityTypeConfiguration<TruckUsage>
    {
        public void Configure(EntityTypeBuilder<TruckUsage> builder)
        {
            builder.ToTable("TruckUsages", "Truck");

            builder.HasKey(tu => tu.Id);

            builder.Property(tu => tu.Id)
                   .IsRequired();

            builder.Property(tu => tu.TruckId)
                   .IsRequired();

            builder.Property(tu => tu.DriverId)
                   .IsRequired();

            builder.Property(tu => tu.StartedAt)
                   .IsRequired();

            builder.Property(tu => tu.EndedAt)
                   .IsRequired(false);

            builder.HasOne(tu => tu.Truck)
                   .WithMany(t => t.TruckUsageHistory)
                   .HasForeignKey(tu => tu.TruckId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tu => tu.Driver)
                   .WithMany(d => d.TruckUsageHistory) 
                   .HasForeignKey(tu => tu.DriverId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

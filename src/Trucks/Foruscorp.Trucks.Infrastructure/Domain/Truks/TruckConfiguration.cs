using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Domain.Truks
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder.ToTable("Trucks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Ulid)
                .IsRequired()
                .HasMaxLength(26); 

            builder.Property(t => t.LicensePlate)
                .IsRequired()
                .HasMaxLength(20); 

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>(); 

            builder.Property(t => t.DriverId)
                .IsRequired(false); 

            // Indexes
            builder.HasIndex(t => t.Ulid)
                .IsUnique(); 

            builder.HasIndex(t => t.LicensePlate)
                .IsUnique(); 

            builder.HasIndex(t => t.DriverId);
        }
    }
}

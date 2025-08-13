using System;
using Foruscorp.Trucks.Domain.Companys;
using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Domain.Trucks
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder.ToTable("Trucks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProviderTruckId);

            builder.Property(t => t.Vin)
                .IsRequired(false);

            builder.Property(t => t.Serial)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(t => t.Make)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(t => t.Model)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(t => t.HarshAccelerationSettingType)
                .IsRequired(false) 
                .HasMaxLength(50);

            builder.Property(t => t.LicensePlate)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(t => t.CreatedAtTime)
                .IsRequired();

            builder.Property(t => t.UpdatedAtTime)
                .IsRequired();

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.DriverId)
                .IsRequired(false);

            builder.Property(t => t.ModelTruckGroupId)
                .IsRequired(false);

            builder.HasOne(t => t.ModelTruckGroup)
                .WithMany(m => m.Trucks)
                .HasForeignKey(t => t.ModelTruckGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(t => t.Vin);

            builder.HasIndex(t => t.Serial);

            builder.HasIndex(t => t.LicensePlate);

            builder.HasIndex(t => t.DriverId);
            builder.HasIndex(t => t.ModelTruckGroupId);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Drivers
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Drivers");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("DriverId")
                .ValueGeneratedOnAdd();

            builder.Property(d => d.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(d => d.TruckId)
                .IsRequired(false);

            builder.Property(d => d.HireDate)
                .IsRequired();

            builder.Property(d => d.ExperienceYears)
                .IsRequired();

            builder.Property(d => d.Bonus)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // One-to-Many relationship with DriverBonus
            builder.HasMany(d => d.Bonuses)
                .WithOne()
                .HasForeignKey(db => db.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.FuelHistories)
                .WithOne()
                .HasForeignKey(fh => fh.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(d => d.LicenseNumber)
                .IsUnique();

            builder.HasIndex(d => d.TruckId);
        }
    }

    public class DriverBonusConfiguration : IEntityTypeConfiguration<DriverBonus>
    {
        public void Configure(EntityTypeBuilder<DriverBonus> builder)
        {
            builder.ToTable("DriverBonuses");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("BonusId");


            builder.Property(b => b.DriverId)
                .IsRequired();

            builder.Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.AwardedAt)
                .IsRequired();

            builder.Property(b => b.Reason)
                .IsRequired()
                .HasMaxLength(200);

            // Index
            builder.HasIndex(b => b.DriverId);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks;
using System.Runtime.CompilerServices;
using Foruscorp.Trucks.Domain.Companys;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Drivers
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Drivers");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id);

            builder.Property(d => d.UserId)
                .IsRequired(false);

            //builder.Property(d => d.FullName)
            //    .IsRequired()
            //    .HasMaxLength(100);

            builder.Property(d => d.LicenseNumber)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(d => d.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(d => d.TruckId)
                .IsRequired(false);

            builder.Property(d => d.HireDate);


            builder.Property(d => d.ExperienceYears);

            builder.Property(d => d.Bonus)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(u => u.Contact)
                   .WithOne()
                   .HasForeignKey<Driver>(u => u.ContactId)
                   .OnDelete(DeleteBehavior.Restrict);

            //builder.OwnsOne(d => d.Contact, b =>
            //{
            //    b.Property(c => c.FullName)
            //        .IsRequired(false);

            //    b.Property(c => c.Phone)
            //        .IsRequired(false)
            //        .HasMaxLength(50);

            //    b.Property(c => c.Email)
            //        .IsRequired(false)
            //        .HasMaxLength(100);

            //    b.Property(c => c.TelegramLink)
            //        .IsRequired(false)
            //        .HasMaxLength(100);
            //});


            builder.HasMany(d => d.Bonuses)
                .WithOne()
                .HasForeignKey(db => db.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.FuelHistories)
                .WithOne()
                .HasForeignKey(fh => fh.DriverId)
                .OnDelete(DeleteBehavior.Cascade);


            //builder.HasOne(d => d.Truck)
            //    .WithOne(t => t.Driver)
            //    .HasForeignKey<Truck>();

            builder.HasOne(d => d.Truck)
                .WithOne(t => t.Driver)
                .HasForeignKey<Driver>(d => d.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne<Company>()
            //    .WithMany()
            //    .HasForeignKey(d => d.CompanyId);

            // Indexes
            builder.HasIndex(d => d.LicenseNumber)
                .IsUnique();

            builder.HasIndex(d => d.TruckId);

            builder.HasIndex(d => d.UserId);
        }
    }

    public class DriverBonusConfiguration : IEntityTypeConfiguration<DriverBonus>
    {
        public void Configure(EntityTypeBuilder<DriverBonus> builder)
        {
            builder.ToTable("DriverBonuses");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserId);

            builder.Property(b => b.Id)
                .HasColumnName("BonusId");

            builder.Property(b => b.DriverId)
                .IsRequired();

            builder.Property(b => b.Amount)
                .IsRequired();

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
using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Domain.Trucks
{
    public class ModelTruckGroupConfiguration : IEntityTypeConfiguration<ModelTruckGroup>
    {
        public void Configure(EntityTypeBuilder<ModelTruckGroup> builder)
        {
            builder.ToTable("ModelTruckGroups");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .IsRequired();

            builder.Property(m => m.TruckGrouName)
                .IsRequired();

            builder.Property(m => m.Make)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Model)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Year)
                .IsRequired();

            builder.Property(m => m.FuelCapacity)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.AverageFuelConsumption)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(m => m.Trucks)
                .WithOne()
                .HasForeignKey(t => t.ModelTruckGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.HasIndex(m => m.TruckGrouName);
            builder.HasIndex(m => m.Make);
            builder.HasIndex(m => m.Model);
            builder.HasIndex(m => m.Year);
            //builder.HasIndex(m => new { m.Make, m.Model, m.Year }).IsUnique();

        }
    }
}

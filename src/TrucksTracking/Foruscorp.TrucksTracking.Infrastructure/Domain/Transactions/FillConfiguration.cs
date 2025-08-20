using Foruscorp.TrucksTracking.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Transactions
{
    public class FillConfiguration : IEntityTypeConfiguration<Fill>
    {
        public void Configure(EntityTypeBuilder<Fill> builder)
        {
            builder.ToTable("Fills");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedNever();

            builder.Property(x => x.TranDate)
                   .IsRequired();

            builder.Property(x => x.TranTime)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.Invoice)
                   .IsRequired();

            builder.Property(x => x.Unit)
                   .IsRequired();

            builder.Property(x => x.Driver)
                   .IsRequired();

            builder.Property(x => x.Odometer)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.Location)
                   .IsRequired();

            builder.Property(x => x.City)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.State)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.HasMany(x => x.Items)
                   .WithOne()
                   .HasForeignKey("FillId")
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Invoice);
            builder.HasIndex(x => x.Unit);
            builder.HasIndex(x => x.Driver);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}

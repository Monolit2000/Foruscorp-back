using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence.Configurations
{
    public class TransactionFillConfiguration : IEntityTypeConfiguration<TransactionFill>
    {
        public void Configure(EntityTypeBuilder<TransactionFill> builder)
        {
            builder.ToTable("TransactionFills", "TuckTracking");

            builder.HasKey(tf => tf.Id);

            builder.Property(tf => tf.TransactionReportId)
                .IsRequired();

            builder.Property(tf => tf.TranDate)
                .HasMaxLength(20);

            builder.Property(tf => tf.TranTime)
                .HasMaxLength(20);

            builder.Property(tf => tf.Invoice)
                .HasMaxLength(50);

            builder.Property(tf => tf.Unit)
                .HasMaxLength(20);

            builder.Property(tf => tf.Driver)
                .HasMaxLength(100);

            builder.Property(tf => tf.Odometer)
                .HasMaxLength(20);

            builder.Property(tf => tf.Location)
                .HasMaxLength(200);

            builder.Property(tf => tf.City)
                .HasMaxLength(100);

            builder.Property(tf => tf.State)
                .HasMaxLength(50);

            // Configure the relationship with TransactionItem
            builder.HasMany(tf => tf.Items)
                .WithOne()
                .HasForeignKey("TransactionFillId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

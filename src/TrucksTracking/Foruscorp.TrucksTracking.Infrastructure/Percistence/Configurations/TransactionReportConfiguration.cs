using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence.Configurations
{
    public class TransactionReportConfiguration : IEntityTypeConfiguration<TransactionReport>
    {
        public void Configure(EntityTypeBuilder<TransactionReport> builder)
        {
            builder.ToTable("TransactionReports", "TuckTracking");

            builder.HasKey(tr => tr.Id);

            builder.Property(tr => tr.Card)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(tr => tr.Group)
                .HasMaxLength(10);

            builder.Property(tr => tr.CreatedAt)
                .IsRequired();

            // Configure the relationship with TransactionFill
            builder.HasMany(tr => tr.Fills)
                .WithOne()
                .HasForeignKey("TransactionReportId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the Summaries as JSON
            builder.Property(tr => tr.Summaries)
                .HasColumnType("jsonb");
        }
    }
}

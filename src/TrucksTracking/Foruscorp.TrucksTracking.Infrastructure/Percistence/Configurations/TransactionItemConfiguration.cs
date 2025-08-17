using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence.Configurations
{
    public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
    {
        public void Configure(EntityTypeBuilder<TransactionItem> builder)
        {
            builder.ToTable("TransactionItems", "TuckTracking");

            builder.HasKey(ti => ti.Id);

            builder.Property(ti => ti.TransactionFillId)
                .IsRequired();

            builder.Property(ti => ti.Type)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(ti => ti.UnitPrice)
                .HasColumnType("decimal(18,4)");

            builder.Property(ti => ti.Quantity)
                .HasColumnType("decimal(18,4)");

            builder.Property(ti => ti.Amount)
                .HasColumnType("decimal(18,4)");

            builder.Property(ti => ti.DB)
                .HasMaxLength(5);

            builder.Property(ti => ti.Currency)
                .HasMaxLength(10);
        }
    }
}

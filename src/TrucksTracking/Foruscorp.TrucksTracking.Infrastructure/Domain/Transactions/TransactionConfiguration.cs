using Foruscorp.TrucksTracking.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Transactions
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedNever();

            builder.Property(x => x.Card)
                   .IsRequired();

            builder.Property(x => x.Group)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.IsProcessed)
                   .IsRequired();

            builder.HasMany(x => x.Fills)
                   .WithOne()
                   .HasForeignKey("TransactionId")
                   .OnDelete(DeleteBehavior.Cascade);

            //builder.Property(x => x.Summaries)
            //       .HasColumnType("jsonb")
            //       .IsRequired(false);

            builder.HasIndex(x => x.Card);
            builder.HasIndex(x => x.Group);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}

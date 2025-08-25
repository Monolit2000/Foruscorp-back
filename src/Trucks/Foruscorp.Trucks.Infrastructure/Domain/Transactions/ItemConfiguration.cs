using Foruscorp.Trucks.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Infrastructure.Domain.Transactions
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedNever();

            builder.Property(x => x.Type)
                   .IsRequired();

            builder.Property(x => x.UnitPrice)
                   .IsRequired();

            builder.Property(x => x.Quantity)
                   .IsRequired();

            builder.Property(x => x.Amount)
                   .IsRequired();

            builder.Property(x => x.DB)
                   .IsRequired();

            builder.Property(x => x.Currency)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.HasIndex(x => x.Type);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}

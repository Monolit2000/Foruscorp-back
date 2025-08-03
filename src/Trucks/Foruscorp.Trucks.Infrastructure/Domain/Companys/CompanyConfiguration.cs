using Foruscorp.Trucks.Domain.Companys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Infrastructure.Domain.Companys
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companys"); 

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .IsRequired();

            builder.Property(c => c.ExternalToken)
                .IsRequired(false);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Cnpj)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(c => c.Phone)
                .IsRequired(false)
                .HasMaxLength(30);

            builder.Property(c => c.Address)
                .IsRequired(false)
                .HasMaxLength(300);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired();

            builder.HasMany(c => c.Trucks)
                .WithOne()
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Drivers)
                .WithOne()
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

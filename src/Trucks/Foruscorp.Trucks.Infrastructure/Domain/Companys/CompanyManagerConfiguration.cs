using Foruscorp.Trucks.Domain.Companys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Trucks.Infrastructure.Domain.Companys
{
    internal class CompanyManagerConfiguration : IEntityTypeConfiguration<CompanyManager>
    {
        public void Configure(EntityTypeBuilder<CompanyManager> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Id)
                   .ValueGeneratedNever()
                   .IsRequired();

            builder.HasIndex(cm => new { cm.UserId, cm.CompanyId })
                   .IsUnique();

            builder.Property(cm => cm.CreatedAt);

            builder.Property(cm => cm.UpdatedAt);

            builder.HasOne(cm => cm.User)
                   .WithMany()
                   .HasForeignKey(cm => cm.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.Company)
                   .WithMany(c => c.CompanyManagers)
                   .HasForeignKey(cm => cm.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cm => cm.UserId);

            builder.HasIndex(cm => cm.CompanyId);
        }
    }
}

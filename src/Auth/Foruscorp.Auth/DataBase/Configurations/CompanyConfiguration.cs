using Foruscorp.Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foruscorp.Auth.DataBase.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companys");

            builder.HasKey(c => c.CompanyId);

            builder.Property(c => c.CompanyId)
                   .IsRequired();

            builder.Property(c => c.Name);

            builder.HasMany(c => c.Users)
                   .WithOne()
                   .HasForeignKey(u => u.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

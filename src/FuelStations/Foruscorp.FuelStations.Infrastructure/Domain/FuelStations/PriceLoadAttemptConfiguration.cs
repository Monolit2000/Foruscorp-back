using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.FuelStations.Domain.FuelStations;

namespace Foruscorp.FuelStations.Infrastructure.Domain.FuelStations
{
    public class PriceLoadAttemptConfiguration : IEntityTypeConfiguration<PriceLoadAttempt>
    {
        public void Configure(EntityTypeBuilder<PriceLoadAttempt> builder)
        {
            builder.ToTable("PriceLoadAttempts");

            builder.HasKey(pla => pla.Id);

            builder.Property(pla => pla.Id)
                .HasColumnName("PriceLoadAttemptId")
                .ValueGeneratedNever();

            builder.Property(pla => pla.StartedAt);

            builder.Property(pla => pla.CompletedAt);

            builder.Property(pla => pla.IsSuccessful)
                .IsRequired();

            builder.Property(pla => pla.ErrorMessage)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.Property(pla => pla.TotalFiles)
                .IsRequired();

            builder.Property(pla => pla.SuccessfullyProcessedFiles)
                .IsRequired();

            builder.Property(pla => pla.FailedFiles)
                .IsRequired();

            // Индексы для быстрого поиска
            builder.HasIndex(pla => pla.StartedAt);
            builder.HasIndex(pla => pla.IsSuccessful);
            builder.HasIndex(pla => pla.CompletedAt);

            builder.OwnsMany(pla => pla.FileResults, frBuilder =>
            {
                frBuilder.ToTable("FileProcessingResults");

                frBuilder.WithOwner()
                    .HasForeignKey("PriceLoadAttemptId");

                frBuilder.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                frBuilder.HasKey("Id");

                frBuilder.Property(fr => fr.FileName)
                    .IsRequired()
                    .HasMaxLength(500);

                frBuilder.Property(fr => fr.IsSuccess)
                    .IsRequired();

                frBuilder.Property(fr => fr.ErrorMessage)
                    .IsRequired(false)
                    .HasMaxLength(2000);

                frBuilder.Property(fr => fr.ProcessedAt);

                frBuilder.HasIndex(fr => fr.FileName);
                frBuilder.HasIndex(fr => fr.IsSuccess);
                frBuilder.HasIndex(fr => fr.ProcessedAt);
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Foruscorp.TrucksTracking.Domain.Reports;

namespace Foruscorp.TrucksTracking.Infrastructure.Domain.Reports
{
    public class ReportLoadAttemptConfiguration : IEntityTypeConfiguration<ReportLoadAttempt>
    {
        public void Configure(EntityTypeBuilder<ReportLoadAttempt> builder)
        {
            builder.ToTable("ReportLoadAttempts");

            builder.HasKey(rla => rla.Id);

            builder.Property(rla => rla.Id)
                .ValueGeneratedNever();

            builder.Property(rla => rla.StartedAt);

            builder.Property(rla => rla.CompletedAt);

            builder.Property(rla => rla.IsSuccessful)
                .IsRequired();

            builder.Property(rla => rla.ErrorMessage)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.Property(rla => rla.TotalFiles)
                .IsRequired();

            builder.Property(rla => rla.SuccessfullyProcessedFiles)
                .IsRequired();

            builder.Property(rla => rla.FailedFiles)
                .IsRequired();

            // Индексы для быстрого поиска
            builder.HasIndex(rla => rla.StartedAt);
            builder.HasIndex(rla => rla.IsSuccessful);
            builder.HasIndex(rla => rla.CompletedAt);

            builder.OwnsMany(rla => rla.FileResults, frBuilder =>
            {
                frBuilder.ToTable("ReportFileProcessingResults");

                frBuilder.WithOwner()
                    .HasForeignKey("ReportLoadAttemptId");

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

                frBuilder.Property(fr => fr.ProcessedAt)
                    .IsRequired();

                // Индексы для файловых результатов
                frBuilder.HasIndex(fr => fr.FileName);
                frBuilder.HasIndex(fr => fr.IsSuccess);
                frBuilder.HasIndex(fr => fr.ProcessedAt);
            });
        }
    }
}

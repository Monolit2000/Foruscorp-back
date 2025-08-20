using System;
using System.Collections.Generic;

namespace Foruscorp.TrucksTracking.Aplication.Reports
{
    public class ReportLoadAttemptDto
    {
        public Guid Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public int TotalFiles { get; set; }
        public int SuccessfullyProcessedFiles { get; set; }
        public int FailedFiles { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? ProcessingDuration { get; set; }
        public List<FileProcessingResultDto> FileResults { get; set; } = new();
    }

    public class FileProcessingResultDto
    {
        public string FileName { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}

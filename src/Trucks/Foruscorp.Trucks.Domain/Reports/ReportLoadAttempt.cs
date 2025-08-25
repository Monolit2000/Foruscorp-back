using Foruscorp.BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Reports
{
    public class ReportLoadAttempt : Entity, IAggregateRoot
    {
        public Guid Id { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string ErrorMessage { get; private set; }
        public int TotalFiles { get; private set; }
        public int SuccessfullyProcessedFiles { get; private set; }
        public int FailedFiles { get; private set; }
        public List<FileProcessingResult> FileResults { get; private set; }

        private ReportLoadAttempt()
        {
            FileResults = new List<FileProcessingResult>();
        }

        private ReportLoadAttempt(int totalFiles)
        {
            Id = Guid.NewGuid();
            StartedAt = DateTime.UtcNow;
            TotalFiles = totalFiles;
            SuccessfullyProcessedFiles = 0;
            FailedFiles = 0;
            IsSuccessful = false;
            FileResults = new List<FileProcessingResult>();
        }

        public static ReportLoadAttempt CreateNew(int totalFiles)
        {
            if (totalFiles < 0)
                throw new ArgumentException("Total files count cannot be negative", nameof(totalFiles));

            return new ReportLoadAttempt(totalFiles);
        }

        public void AddFileResult(string fileName, bool isSuccess, string errorMessage = null)
        {
            var fileResult = new FileProcessingResult(fileName, isSuccess, errorMessage);
            FileResults.Add(fileResult);

            if (isSuccess)
            {
                SuccessfullyProcessedFiles++;
            }
            else
            {
                FailedFiles++;
            }
        }

        public void Complete(bool isSuccessful, string errorMessage = null)
        {
            CompletedAt = DateTime.UtcNow;
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }

        public double GetSuccessRate()
        {
            if (TotalFiles == 0) return 0.0;
            return (double)SuccessfullyProcessedFiles / TotalFiles * 100.0;
        }

        public TimeSpan? GetProcessingDuration()
        {
            if (!CompletedAt.HasValue) return null;
            return CompletedAt.Value - StartedAt;
        }
    }

    public class FileProcessingResult : ValueObject
    {
        public string FileName { get; private set; }
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public DateTime ProcessedAt { get; private set; }

        private FileProcessingResult() { }

        public FileProcessingResult(string fileName, bool isSuccess, string errorMessage = null)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
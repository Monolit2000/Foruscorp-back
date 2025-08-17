using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Foruscorp.TrucksTracking.Aplication.TransactionReports;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Worker.Services
{
    public class PdfProcessingService : BackgroundService
    {
        private readonly ILogger<PdfProcessingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITransactionReportService _transactionReportService;
        private readonly string _pdfWatchDirectory;
        private readonly string _processedDirectory;
        private readonly string _errorDirectory;
        private readonly TimeSpan _pollingInterval;

        public PdfProcessingService(
            ILogger<PdfProcessingService> logger,
            IConfiguration configuration,
            ITransactionReportService transactionReportService)
        {
            _logger = logger;
            _configuration = configuration;
            _transactionReportService = transactionReportService;
            
            _pdfWatchDirectory = _configuration["PdfProcessing:WatchDirectory"] ?? "C:\\PdfInput";
            _processedDirectory = _configuration["PdfProcessing:ProcessedDirectory"] ?? "C:\\PdfProcessed";
            _errorDirectory = _configuration["PdfProcessing:ErrorDirectory"] ?? "C:\\PdfError";
            _pollingInterval = TimeSpan.FromSeconds(
                int.Parse(_configuration["PdfProcessing:PollingIntervalSeconds"] ?? "30"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PDF Processing Service started. Watching directory: {Directory}", _pdfWatchDirectory);

            // Create directories if they don't exist
            Directory.CreateDirectory(_pdfWatchDirectory);
            Directory.CreateDirectory(_processedDirectory);
            Directory.CreateDirectory(_errorDirectory);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPdfFilesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing PDF files");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }

        private async Task ProcessPdfFilesAsync()
        {
            var pdfFiles = Directory.GetFiles(_pdfWatchDirectory, "*.pdf");

            foreach (var pdfFile in pdfFiles)
            {
                try
                {
                    _logger.LogInformation("Processing PDF file: {FileName}", Path.GetFileName(pdfFile));

                    // Parse the PDF
                    var transactions = await _transactionReportService.ParsePdfAsync(pdfFile);

                    // Save transactions to database
                    var savedTransactions = new List<TransactionReport>();
                    foreach (var transaction in transactions)
                    {
                        var savedTransaction = await _transactionReportService.SaveTransactionReportAsync(transaction);
                        savedTransactions.Add(savedTransaction);
                    }

                    _logger.LogInformation("Successfully processed {Count} transactions from {FileName}", 
                        savedTransactions.Count, Path.GetFileName(pdfFile));

                    // Move file to processed directory
                    var processedPath = Path.Combine(_processedDirectory, Path.GetFileName(pdfFile));
                    File.Move(pdfFile, processedPath, true);

                    _logger.LogInformation("Moved {FileName} to processed directory", Path.GetFileName(pdfFile));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing PDF file: {FileName}", Path.GetFileName(pdfFile));

                    // Move file to error directory
                    var errorPath = Path.Combine(_errorDirectory, Path.GetFileName(pdfFile));
                    File.Move(pdfFile, errorPath, true);

                    _logger.LogInformation("Moved {FileName} to error directory", Path.GetFileName(pdfFile));
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PDF Processing Service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foruscorp.TrucksTracking.Aplication.TransactionReports;
using Foruscorp.TrucksTracking.Domain.Trucks;
using System.IO;
using System.Linq;

namespace Foruscorp.TrucksTracking.Examples
{
    /// <summary>
    /// Example class demonstrating how to use the PDF processing functionality
    /// Uses iTextSharp library for PDF text extraction
    /// </summary>
    public class PdfProcessingExample
    {
        private readonly ITransactionReportService _transactionReportService;

        public PdfProcessingExample(ITransactionReportService transactionReportService)
        {
            _transactionReportService = transactionReportService;
        }

        /// <summary>
        /// Example: Process a single PDF file and save to database
        /// </summary>
        public async Task ProcessSinglePdfFileAsync(string pdfFilePath)
        {
            try
            {
                Console.WriteLine($"Processing PDF file: {pdfFilePath}");

                // Parse the PDF file using iTextSharp
                var transactions = await _transactionReportService.ParsePdfAsync(pdfFilePath);

                Console.WriteLine($"Found {transactions.Count} transaction reports");

                // Save each transaction to the database
                foreach (var transaction in transactions)
                {
                    var savedTransaction = await _transactionReportService.SaveTransactionReportAsync(transaction);
                    Console.WriteLine($"Saved transaction for card {savedTransaction.Card}, group {savedTransaction.Group}");
                    
                    // Print details of each fill
                    foreach (var fill in savedTransaction.Fills)
                    {
                        Console.WriteLine($"  Fill: Date={fill.TranDate}, Time={fill.TranTime}, Driver={fill.Driver}");
                        
                        foreach (var item in fill.Items)
                        {
                            Console.WriteLine($"    Item: {item.Type}, Qty={item.Quantity}, Amt={item.Amount}");
                        }
                    }
                }

                Console.WriteLine("PDF processing completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing PDF: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Retrieve and display transaction reports by card number
        /// </summary>
        public async Task DisplayTransactionReportsByCardAsync(string cardNumber)
        {
            try
            {
                Console.WriteLine($"Retrieving transaction reports for card: {cardNumber}");

                var reports = await _transactionReportService.GetTransactionReportsByCardAsync(cardNumber);

                Console.WriteLine($"Found {reports.Count} reports for card {cardNumber}");

                foreach (var report in reports)
                {
                    Console.WriteLine($"Report ID: {report.Id}, Group: {report.Group}, Created: {report.CreatedAt}");
                    Console.WriteLine($"  Fills: {report.Fills.Count}");
                    Console.WriteLine($"  Summaries: {report.Summaries.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving reports: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Process multiple PDF files in a directory
        /// </summary>
        public async Task ProcessMultiplePdfFilesAsync(string directoryPath)
        {
            try
            {
                Console.WriteLine($"Processing PDF files in directory: {directoryPath}");

                var pdfFiles = Directory.GetFiles(directoryPath, "*.pdf");

                Console.WriteLine($"Found {pdfFiles.Length} PDF files");

                var totalTransactions = 0;

                foreach (var pdfFile in pdfFiles)
                {
                    try
                    {
                        Console.WriteLine($"Processing: {Path.GetFileName(pdfFile)}");

                        var transactions = await _transactionReportService.ParsePdfAsync(pdfFile);

                        foreach (var transaction in transactions)
                        {
                            await _transactionReportService.SaveTransactionReportAsync(transaction);
                            totalTransactions++;
                        }

                        Console.WriteLine($"  Processed {transactions.Count} transactions");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error processing {Path.GetFileName(pdfFile)}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Total transactions processed: {totalTransactions}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Generate summary statistics from transaction reports
        /// </summary>
        public async Task GenerateSummaryStatisticsAsync()
        {
            try
            {
                Console.WriteLine("Generating summary statistics...");

                var allReports = await _transactionReportService.GetTransactionReportsAsync();

                var totalReports = allReports.Count;
                var totalFills = allReports.Sum(r => r.Fills.Count);
                var totalItems = allReports.Sum(r => r.Fills.Sum(f => f.Items.Count));

                var totalFuelAmount = allReports.Sum(r => 
                    r.Fills.Sum(f => f.Items.Sum(i => i.Amount)));

                var fuelByType = allReports
                    .SelectMany(r => r.Fills)
                    .SelectMany(f => f.Items)
                    .GroupBy(i => i.Type)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));

                Console.WriteLine($"Summary Statistics:");
                Console.WriteLine($"  Total Reports: {totalReports}");
                Console.WriteLine($"  Total Fills: {totalFills}");
                Console.WriteLine($"  Total Items: {totalItems}");
                Console.WriteLine($"  Total Fuel Amount: ${totalFuelAmount:F2}");

                foreach (var fuelType in fuelByType)
                {
                    Console.WriteLine($"  {fuelType.Key}: ${fuelType.Value:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating statistics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Find transactions by date range (using repository directly)
        /// </summary>
        public async Task FindTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"Finding transactions between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");

                // Note: This would require adding a method to the service interface
                // For now, we'll get all reports and filter in memory
                var allReports = await _transactionReportService.GetTransactionReportsAsync();

                var filteredReports = allReports
                    .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                    .ToList();

                Console.WriteLine($"Found {filteredReports.Count} reports in date range");

                foreach (var report in filteredReports)
                {
                    Console.WriteLine($"  Report: {report.Id}, Card: {report.Card}, Created: {report.CreatedAt:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding transactions: {ex.Message}");
                throw;
            }
        }
    }
}

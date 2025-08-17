using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Foruscorp.TrucksTracking.Aplication.TransactionReports;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Services
{
    public class TransactionReportService : ITransactionReportService
    {
        private readonly ITransactionReportRepository _transactionReportRepository;

        public TransactionReportService(ITransactionReportRepository transactionReportRepository)
        {
            _transactionReportRepository = transactionReportRepository;
        }

        public async Task<List<TransactionReport>> ParsePdfAsync(string pdfPath)
        {
            if (!File.Exists(pdfPath))
            {
                throw new FileNotFoundException($"PDF file not found: {pdfPath}");
            }

            string allText = ExtractTextFromPdf(pdfPath);
            string[] lines = allText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<TransactionReport> transactions = new List<TransactionReport>();

            TransactionReport currentGroup = null;
            TransactionFill currentFill = null;
            int lineIndex = 0;

            while (lineIndex < lines.Length)
            {
                string line = lines[lineIndex].Trim();

                // Skip headers and irrelevant lines
                if (string.IsNullOrEmpty(line) || 
                    line.Contains("Transaction Report") || 
                    line.Contains("Page") || 
                    line.Contains("Carrier:") || 
                    line.Contains("Total Records:") || 
                    line == "Card #" || 
                    line == "Tran Date" || 
                    line == "Tran" || 
                    line == "Time" || 
                    line == "Invoice" || 
                    line == "Unit" || 
                    line == "Driver Name" || 
                    line == "Odometer" || 
                    line == "Location Name" || 
                    line == "City" || 
                    line == "State/" || 
                    line == "Prov" || 
                    line == "Fees" || 
                    line == "Item Unit Price" || 
                    line == "Qty" || 
                    line == "Amt DB Currency" || 
                    line == "Grand Totals" || 
                    line == "Avg PPU" || 
                    line == "Amount" || 
                    line == "Quantity")
                {
                    lineIndex++;
                    continue;
                }

                // Start of a new transaction group or fill
                if (int.TryParse(line, out _) && line.Length == 5) // Card number (5 digits)
                {
                    string card = line;

                    if (currentGroup != null && currentGroup.Card != card)
                    {
                        transactions.Add(currentGroup);
                        currentGroup = null;
                    }

                    if (currentGroup == null)
                    {
                        currentGroup = TransactionReport.CreateNew(card, "");
                    }

                    // Start a new fill within the group
                    currentFill = null;

                    lineIndex++; // Tran Date
                    string tranDate = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    lineIndex++; // Tran Time
                    string tranTime = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    lineIndex++; // Invoice
                    string invoice = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    lineIndex++; // Unit
                    string unit = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    lineIndex++; // Driver and Odometer (e.g., "DMYTRO SUTULIN 432")
                    string driver = "";
                    string odometer = "";
                    if (lineIndex < lines.Length)
                    {
                        string driverLine = lines[lineIndex].Trim();
                        var parts = driverLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        odometer = parts.Last();
                        driver = string.Join(" ", parts.Take(parts.Length - 1));
                    }

                    lineIndex++; // Location
                    string location = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    lineIndex++; // City
                    string city = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    lineIndex++; // State
                    string state = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                    currentFill = TransactionFill.CreateNew(
                        currentGroup.Id,
                        tranDate,
                        tranTime,
                        invoice,
                        unit,
                        driver,
                        odometer,
                        location,
                        city,
                        state);

                    currentGroup.AddFill(currentFill);
                }
                // Item lines (ULSD or DEFD)
                else if (line == "ULSD" || line == "DEFD")
                {
                    if (currentFill != null)
                    {
                        string type = line;

                        lineIndex++; // Unit Price
                        double unitPrice = 0;
                        if (lineIndex < lines.Length && double.TryParse(lines[lineIndex].Trim(), out double up)) 
                            unitPrice = up;

                        lineIndex++; // Quantity
                        double quantity = 0;
                        if (lineIndex < lines.Length && double.TryParse(lines[lineIndex].Trim(), out double qty)) 
                            quantity = qty;

                        lineIndex++; // Amount
                        double amount = 0;
                        if (lineIndex < lines.Length && double.TryParse(lines[lineIndex].Trim(), out double amt)) 
                            amount = amt;

                        lineIndex++; // DB (Y/N)
                        string db = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                        lineIndex++; // Currency
                        string currency = lineIndex < lines.Length ? lines[lineIndex].Trim() : "";

                        var item = TransactionItem.CreateNew(
                            currentFill.Id,
                            type,
                            unitPrice,
                            quantity,
                            amount,
                            db,
                            currency);

                        currentFill.AddItem(item);
                    }
                }
                // Summary section (after items, per group)
                else if (line == "Amount" && currentGroup != null)
                {
                    lineIndex++; // Skip to next (Quantity, but we process summaries)
                    while (lineIndex < lines.Length)
                    {
                        line = lines[lineIndex].Trim();

                        if (line == "DEFD" || line == "ULSD")
                        {
                            var sumKey = line.ToLower() + "_sum";
                            double amt = 0, qty = 0, ppu = 0;
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out amt);
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out qty);
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out ppu);
                            currentGroup.AddSummary(sumKey, new { Amount = amt, Quantity = qty, AvgPPU = ppu });
                        }
                        else if (line == "Fees")
                        {
                            double feeAmt = 0, feeQty = 0;
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out feeAmt);
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out feeQty);
                            currentGroup.AddSummary("fees", new { Amount = feeAmt, Quantity = feeQty });
                        }
                        else if (line == "Totals")
                        {
                            double totAmt = 0, totQty = 0;
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out totAmt);
                            lineIndex++; if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out totQty);
                            currentGroup.AddSummary("totals", new { Amount = totAmt, Quantity = totQty });
                        }
                        else if (line == "Total Fuel")
                        {
                            lineIndex++; // Amount on next line
                            double fuelAmt = 0;
                            if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out fuelAmt);
                            lineIndex++; // Quantity
                            double fuelQty = 0;
                            if (lineIndex < lines.Length) double.TryParse(lines[lineIndex].Trim(), out fuelQty);
                            currentGroup.AddSummary("total_fuel", new { Amount = fuelAmt, Quantity = fuelQty });
                        }
                        else if (int.TryParse(line, out int groupNum) && groupNum >= 1 && groupNum <= 99) // Group number (1-14 in example)
                        {
                            currentGroup = TransactionReport.CreateNew(currentGroup.Card, line);
                            lineIndex += 2; // Skip repeated card and "Group:"
                            break; // End of summary
                        }
                        else
                        {
                            lineIndex++;
                        }
                    }
                }
                else
                {
                    lineIndex++;
                }
            }

            if (currentGroup != null)
            {
                transactions.Add(currentGroup);
            }

            return transactions;
        }

        public async Task<TransactionReport> SaveTransactionReportAsync(TransactionReport transactionReport)
        {
            return await _transactionReportRepository.AddAsync(transactionReport);
        }

        public async Task<List<TransactionReport>> GetTransactionReportsAsync()
        {
            var reports = await _transactionReportRepository.GetAllAsync();
            return reports.ToList();
        }

        public async Task<TransactionReport> GetTransactionReportByIdAsync(Guid id)
        {
            return await _transactionReportRepository.GetByIdAsync(id);
        }

        public async Task<List<TransactionReport>> GetTransactionReportsByCardAsync(string card)
        {
            var reports = await _transactionReportRepository.GetByCardAsync(card);
            return reports.ToList();
        }

        private static string ExtractTextFromPdf(string pdfPath)
        {
            using (PdfReader reader = new PdfReader(pdfPath))
            {
                StringBuilder text = new StringBuilder();
                
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, i);
                    text.AppendLine(pageText);
                }
                
                return text.ToString();
            }
        }
    }
}

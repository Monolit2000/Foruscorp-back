using Foruscorp.TrucksTracking.Aplication.Contructs.Services;
using Foruscorp.TrucksTracking.Domain.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Foruscorp.TrucksTracking.Infrastructure.Services
{
    public class PdfTransactionService : IPdfTransactionService
    {
        private readonly ILogger<PdfTransactionService> _logger;

        public PdfTransactionService(ILogger<PdfTransactionService> logger)
        {
            _logger = logger;
        }

        public async Task<List<Transaction>> ParsePdfTransactionsAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("PDF file is null or empty");
                return new List<Transaction>();
            }

            try
            {
                _logger.LogInformation("Starting to parse PDF file: {FileName}", file.FileName);

                // Сохраняем файл во временную директорию
                var tempPath = Path.GetTempFileName();
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                var transactions = await ParsePdfTransactionsFromPathAsync(tempPath, cancellationToken);

                // Удаляем временный файл
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                _logger.LogInformation("Successfully parsed {TransactionCount} transactions from PDF file: {FileName}", 
                    transactions.Count, file.FileName);

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PDF file: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<List<Transaction>> ParsePdfTransactionsFromPathAsync(string pdfPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
            {
                _logger.LogWarning("PDF file does not exist: {PdfPath}", pdfPath);
                return new List<Transaction>();
            }

            try
            {
                _logger.LogInformation("Starting to parse PDF from path: {PdfPath}", pdfPath);

                string allText = ExtractTextFromPdf(pdfPath);
                string[] lines = allText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                List<Transaction> transactions = new List<Transaction>();

                Transaction currentGroup = null;
                Fill currentFill = null;
                int lineIndex = 0;

                while (lineIndex < lines.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string line = lines[lineIndex].Trim();

                    // Skip headers and irrelevant lines
                    if (ShouldSkipLine(line))
                    {
                        lineIndex++;
                        continue;
                    }

                    // Start of a new transaction group or fill
                    if (IsCardNumberLine(line))
                    {
                        string card = ExtractCardNumber(line);

                        if (currentGroup != null && currentGroup.Card != card)
                        {
                            transactions.Add(currentGroup);
                            currentGroup = null;
                        }

                        if (currentGroup == null)
                        {
                            currentGroup = Transaction.CreateNew(card, string.Empty);
                        }

                        // Start a new fill within the group
                        currentFill = CreateFillFromLines(lines, ref lineIndex);
                        currentGroup.AddFill(currentFill);
                    }
                    // Item lines (ULSD or DEFD)
                    else if (line == "ULSD" || line == "DEFD")
                    {
                        if (currentFill != null)
                        {
                            var item = CreateItemFromLines(lines, ref lineIndex, line);
                            currentFill.AddItem(item);
                        }
                    }
                    // Summary section (after items, per group)
                    else if (line == "Amount" && currentGroup != null)
                    {
                        ProcessSummarySection(lines, ref lineIndex, currentGroup);
                    }
                    else
                    {
                        lineIndex++;
                    }
                    lineIndex++;
                }

                if (currentGroup != null)
                {
                    transactions.Add(currentGroup);
                }

                _logger.LogInformation("Successfully parsed {TransactionCount} transactions from PDF", transactions.Count);
                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PDF from path: {PdfPath}", pdfPath);
                throw;
            }
        }

        private bool ShouldSkipLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return true;

            var skipKeywords = new[]
            {
                "Transaction Report", "Page", "Carrier:", "Total Records:", "Card #", "Tran Date", "Tran", "Time",
                "Invoice", "Unit", "Driver Name", "Odometer", "Location Name", "City", "State/", "Prov", "Fees",
                "Item Unit Price", "Qty", "Amt DB Currency", "Grand Totals", "Avg PPU", "Amount", "Quantity"
            };

            return skipKeywords.Any(keyword => line.Contains(keyword));
        }

        private Fill CreateFillFromLines(string[] lines, ref int lineIndex)
        {
            // Получаем текущую строку, которая содержит всю информацию о заправке
            string currentLine = lines[lineIndex];
            var parts = currentLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Парсим данные из строки: "47276 2025-06-04 11:36 16931 USC999 YAROSLAV DOVHOBORETS LOVES #881 TRAVEL STOP DIAMOND OH"
            // parts[0] = card number (47276)
            // parts[1] = date (2025-06-04)
            // parts[2] = time (11:36)
            // parts[3] = invoice (16931)
            // parts[4] = unit (USC999)
            // parts[5..] = driver name + location info
            
            string tranDate = parts.Length > 1 ? parts[1] : string.Empty;
            string tranTime = parts.Length > 2 ? parts[2] : string.Empty;
            string invoice = parts.Length > 3 ? parts[3] : string.Empty;
            string unit = parts.Length > 4 ? parts[4] : string.Empty;
            
            // Извлекаем имя водителя и информацию о местоположении
            string driver = string.Empty;
            string location = string.Empty;
            string city = string.Empty;
            string state = string.Empty;
            
            if (parts.Length > 5)
            {
                // Ищем последние части для города и штата
                var locationParts = new List<string>();
                for (int i = 5; i < parts.Length; i++)
                {
                    locationParts.Add(parts[i]);
                }
                
                // Предполагаем, что последние 2-3 части - это город и штат
                if (locationParts.Count >= 2)
                {
                    state = locationParts[locationParts.Count - 1]; // Последний элемент - штат
                    city = locationParts[locationParts.Count - 2]; // Предпоследний - город
                    
                    // Остальное - это имя водителя и название локации
                    var driverAndLocation = locationParts.Take(locationParts.Count - 2).ToList();
                    if (driverAndLocation.Count > 0)
                    {
                        // Первые слова - имя водителя, остальное - название локации
                        driver = string.Join(" ", driverAndLocation.Take(Math.Min(3, driverAndLocation.Count)));
                        location = string.Join(" ", driverAndLocation.Skip(Math.Min(3, driverAndLocation.Count)));
                    }
                }
                else
                {
                    driver = string.Join(" ", locationParts);
                }
            }
            
            // Извлекаем одометр из следующей строки (если есть)
            string odometer = string.Empty;
            if (lineIndex + 1 < lines.Length)
            {
                var nextLine = lines[lineIndex + 1].Trim();
                if (nextLine.Contains("Odometer") || nextLine.Contains("ODO"))
                {
                    odometer = ExtractOdometerFromLine(nextLine);
                    lineIndex++; // Пропускаем строку с одометром
                }
            }

            var fill = Fill.CreateNew(
                tranDate: tranDate,
                tranTime: tranTime,
                invoice: invoice,
                unit: unit,
                driver: driver,
                odometer: odometer,
                location: location,
                city: city,
                state: state
            );

            return fill;
        }

        private Item CreateItemFromLines(string[] lines, ref int lineIndex, string itemType)
        {
            // Получаем текущую строку с данными товара
            string currentLine = lines[lineIndex];
            var parts = currentLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Парсим данные из строки: "ULSD 3.249 127.13 413.03 Y USD/Gallons"
            // parts[0] = item type (ULSD)
            // parts[1] = unit price (3.249)
            // parts[2] = quantity (127.13)
            // parts[3] = amount (413.03)
            // parts[4] = db (Y)
            // parts[5] = currency (USD/Gallons)
            
            double unitPrice = parts.Length > 1 ? ParseDouble(parts[1]) : 0.0;
            double quantity = parts.Length > 2 ? ParseDouble(parts[2]) : 0.0;
            double amount = parts.Length > 3 ? ParseDouble(parts[3]) : 0.0;
            string db = parts.Length > 4 ? parts[4] : string.Empty;
            string currency = parts.Length > 5 ? parts[5] : string.Empty;

            return Item.CreateNew(itemType, unitPrice, quantity, amount, db, currency);
        }

        private void ProcessSummarySection(string[] lines, ref int lineIndex, Transaction currentGroup)
        {
            lineIndex++; // Skip to next (Quantity, but we process summaries)
            while (lineIndex < lines.Length)
            {
                string line = lines[lineIndex].Trim();

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
                else if (int.TryParse(line, out int groupNum) && groupNum >= 1 && groupNum <= 99) // Group number
                {
                    // Обновляем группу в транзакции
                    // Note: В доменной модели Group - это строка, поэтому мы не можем изменить её после создания
                    // В реальном приложении можно добавить метод для обновления группы
                    lineIndex += 2; // Skip repeated card and "Group:"
                    break; // End of summary
                }
                else
                {
                    lineIndex++;
                }
            }
        }

        private string GetNextLine(string[] lines, ref int lineIndex)
        {
            lineIndex++;
            return lineIndex < lines.Length ? lines[lineIndex].Trim() : string.Empty;
        }

        private string ExtractDriverFromLine(string driverLine)
        {
            if (string.IsNullOrWhiteSpace(driverLine)) return string.Empty;

            var parts = driverLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Take(parts.Length - 1));
        }

        private string ExtractOdometerFromLine(string driverLine)
        {
            if (string.IsNullOrWhiteSpace(driverLine)) return string.Empty;

            var parts = driverLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Last();
        }

        private double ParseDouble(string value)
        {
            return double.TryParse(value, out double result) ? result : 0.0;
        }

        private bool IsCardNumberLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            
            // Проверяем, что строка начинается с 5-значного числа
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;
            
            return int.TryParse(parts[0], out int cardNumber) && 
                   parts[0].Length == 5 && 
                   cardNumber >= 10000 && 
                   cardNumber <= 99999;
        }

        private string ExtractCardNumber(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private string ExtractTextFromPdf(string pdfPath)
        {
            using (PdfReader reader = new PdfReader(pdfPath))
            using (PdfDocument document = new PdfDocument(reader))
            {
                var text = new StringBuilder();
                for (int i = 1; i <= document.GetNumberOfPages(); i++)
                {
                    var page = document.GetPage(i);
                    var strategy = new LocationTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    text.AppendLine(currentText);
                }
                return text.ToString();
            }
        }
    }
}

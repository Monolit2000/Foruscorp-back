using FluentResults;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contructs.Services;
using Foruscorp.TrucksTracking.Domain.Transactions;
using Foruscorp.TrucksTracking.Domain.Reports;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Transactions.ParsePdfTransactions
{
    public record ParsePdfTransactionsCommand(IFormFile File) : IRequest<Result<List<TransactionDto>>>;

    public class ParsePdfTransactionsCommandHandler : IRequestHandler<ParsePdfTransactionsCommand, Result<List<TransactionDto>>>
    {
        private readonly IPdfTransactionService _pdfTransactionService;
        private readonly ILogger<ParsePdfTransactionsCommandHandler> _logger;
        private readonly ITruckTrackingContext _truckTrackingContext;

        public ParsePdfTransactionsCommandHandler(
            IPdfTransactionService pdfTransactionService,
            ILogger<ParsePdfTransactionsCommandHandler> logger,
            ITruckTrackingContext truckTrackingContext)
        {
            _truckTrackingContext = truckTrackingContext;
            _pdfTransactionService = pdfTransactionService;
            _logger = logger;
        }

        public async Task<Result<List<TransactionDto>>> Handle(ParsePdfTransactionsCommand request, CancellationToken cancellationToken)
        {
            // Создаем запись о попытке загрузки отчета
            var reportLoadAttempt = ReportLoadAttempt.CreateNew(1); // 1 файл
            await _truckTrackingContext.ReportLoadAttempts.AddAsync(reportLoadAttempt, cancellationToken);
            await _truckTrackingContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Started report load attempt {AttemptId} with file: {FileName}", 
                reportLoadAttempt.Id, request.File?.FileName);

            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    var errorMessage = "PDF file is required and cannot be empty";
                    reportLoadAttempt.AddFileResult(request.File?.FileName ?? "null", false, errorMessage);
                    reportLoadAttempt.Complete(false, errorMessage);
                    await _truckTrackingContext.SaveChangesAsync(cancellationToken);
                    return Result.Fail(errorMessage);
                }

                if (!request.File.FileName.ToLower().EndsWith(".pdf"))
                {
                    var errorMessage = "File must be a PDF document";
                    reportLoadAttempt.AddFileResult(request.File.FileName, false, errorMessage);
                    reportLoadAttempt.Complete(false, errorMessage);
                    await _truckTrackingContext.SaveChangesAsync(cancellationToken);
                    return Result.Fail(errorMessage);
                }

                _logger.LogInformation("Starting to parse PDF transactions from file: {FileName}", request.File.FileName);

                var transactions = await _pdfTransactionService.ParsePdfTransactionsAsync(request.File, cancellationToken);

                if (!transactions.Any())
                {
                    _logger.LogWarning("No transactions found in PDF file: {FileName}", request.File.FileName);
                    reportLoadAttempt.AddFileResult(request.File.FileName, true, "No transactions found");
                    reportLoadAttempt.Complete(true);
                    await _truckTrackingContext.SaveChangesAsync(cancellationToken);
                    return Result.Ok(new List<TransactionDto>());
                }

                await _truckTrackingContext.Transactions.AddRangeAsync(transactions, cancellationToken);
                await _truckTrackingContext.SaveChangesAsync(cancellationToken);

                var transactionDtos = transactions.Select(MapToDto).ToList();

                _logger.LogInformation("Successfully parsed {TransactionCount} transactions from PDF file: {FileName}", 
                    transactionDtos.Count, request.File.FileName);

                // Добавляем успешный результат
                reportLoadAttempt.AddFileResult(request.File.FileName, true);
                reportLoadAttempt.Complete(true);
                await _truckTrackingContext.SaveChangesAsync(cancellationToken);




                return Result.Ok(transactionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PDF transactions from file: {FileName}", request.File?.FileName);
                var errorMessage = $"Error parsing PDF transactions: {ex.Message}";
                
                reportLoadAttempt.AddFileResult(request.File?.FileName ?? "unknown", false, errorMessage);
                reportLoadAttempt.Complete(false, errorMessage);
                await _truckTrackingContext.SaveChangesAsync(cancellationToken);
                
                return Result.Fail(errorMessage);
            }
        }

        private TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                Card = transaction.Card,
                Group = transaction.Group,
                CreatedAt = transaction.CreatedAt,
                Fills = transaction.Fills.Select(MapToFillDto).ToList(),
                Summaries = transaction.Summaries
            };
        }

        private FillDto MapToFillDto(Fill fill)
        {
            return new FillDto
            {
                Id = fill.Id,
                TranDate = fill.TranDate,
                TranTime = fill.TranTime,
                Invoice = fill.Invoice,
                Unit = fill.Unit,
                Driver = fill.Driver,
                Odometer = fill.Odometer,
                Location = fill.Location,
                City = fill.City,
                State = fill.State,
                CreatedAt = fill.CreatedAt,
                Items = fill.Items.Select(MapToItemDto).ToList()
            };
        }

        private ItemDto MapToItemDto(Item item)
        {
            return new ItemDto
            {
                Id = item.Id,
                Type = item.Type,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Amount = item.Amount,
                DB = item.DB,
                Currency = item.Currency,
                CreatedAt = item.CreatedAt
            };
        }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string Card { get; set; }
        public string Group { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<FillDto> Fills { get; set; } = new();
        public Dictionary<string, object> Summaries { get; set; } = new();
    }

    public class FillDto
    {
        public Guid Id { get; set; }
        public string TranDate { get; set; }
        public string TranTime { get; set; }
        public string Invoice { get; set; }
        public string Unit { get; set; }
        public string Driver { get; set; }
        public string Odometer { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }

    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public double UnitPrice { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public string DB { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

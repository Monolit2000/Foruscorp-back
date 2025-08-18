using Foruscorp.TrucksTracking.Domain.Transactions;
using Microsoft.AspNetCore.Http;

namespace Foruscorp.TrucksTracking.Aplication.Contructs.Services
{
    public interface IPdfTransactionService
    {
        Task<List<Transaction>> ParsePdfTransactionsAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<List<Transaction>> ParsePdfTransactionsFromStreamAsync(Stream pdfStream, CancellationToken cancellationToken = default);
        Task<List<Transaction>> ParsePdfTransactionsFromPathAsync(string pdfPath, CancellationToken cancellationToken = default);
    }
}

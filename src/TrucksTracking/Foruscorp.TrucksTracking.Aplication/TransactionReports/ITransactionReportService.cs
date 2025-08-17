using System.Collections.Generic;
using System.Threading.Tasks;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.TransactionReports
{
    public interface ITransactionReportService
    {
        Task<List<TransactionReport>> ParsePdfAsync(string pdfPath);
        Task<TransactionReport> SaveTransactionReportAsync(TransactionReport transactionReport);
        Task<List<TransactionReport>> GetTransactionReportsAsync();
        Task<TransactionReport> GetTransactionReportByIdAsync(Guid id);
        Task<List<TransactionReport>> GetTransactionReportsByCardAsync(string card);
    }
}

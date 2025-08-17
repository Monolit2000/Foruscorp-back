using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public interface ITransactionReportRepository
    {
        Task<TransactionReport> GetByIdAsync(Guid id);
        Task<IEnumerable<TransactionReport>> GetAllAsync();
        Task<IEnumerable<TransactionReport>> GetByCardAsync(string card);
        Task<IEnumerable<TransactionReport>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<TransactionReport> AddAsync(TransactionReport transactionReport);
        Task<TransactionReport> UpdateAsync(TransactionReport transactionReport);
        Task DeleteAsync(Guid id);
    }
}

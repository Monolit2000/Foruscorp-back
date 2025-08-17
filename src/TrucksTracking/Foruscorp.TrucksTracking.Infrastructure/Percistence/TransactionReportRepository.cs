using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence
{
    public class TransactionReportRepository : ITransactionReportRepository
    {
        private readonly TuckTrackingContext _context;

        public TransactionReportRepository(TuckTrackingContext context)
        {
            _context = context;
        }

        public async Task<TransactionReport> GetByIdAsync(Guid id)
        {
            return await _context.TransactionReports
                .Include(tr => tr.Fills)
                .ThenInclude(f => f.Items)
                .FirstOrDefaultAsync(tr => tr.Id == id);
        }

        public async Task<IEnumerable<TransactionReport>> GetAllAsync()
        {
            return await _context.TransactionReports
                .Include(tr => tr.Fills)
                .ThenInclude(f => f.Items)
                .ToListAsync();
        }

        public async Task<IEnumerable<TransactionReport>> GetByCardAsync(string card)
        {
            return await _context.TransactionReports
                .Include(tr => tr.Fills)
                .ThenInclude(f => f.Items)
                .Where(tr => tr.Card == card)
                .ToListAsync();
        }

        public async Task<IEnumerable<TransactionReport>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TransactionReports
                .Include(tr => tr.Fills)
                .ThenInclude(f => f.Items)
                .Where(tr => tr.CreatedAt >= startDate && tr.CreatedAt <= endDate)
                .ToListAsync();
        }

        public async Task<TransactionReport> AddAsync(TransactionReport transactionReport)
        {
            await _context.TransactionReports.AddAsync(transactionReport);
            await _context.SaveChangesAsync();
            return transactionReport;
        }

        public async Task<TransactionReport> UpdateAsync(TransactionReport transactionReport)
        {
            _context.TransactionReports.Update(transactionReport);
            await _context.SaveChangesAsync();
            return transactionReport;
        }

        public async Task DeleteAsync(Guid id)
        {
            var transactionReport = await _context.TransactionReports.FindAsync(id);
            if (transactionReport != null)
            {
                _context.TransactionReports.Remove(transactionReport);
                await _context.SaveChangesAsync();
            }
        }
    }
}

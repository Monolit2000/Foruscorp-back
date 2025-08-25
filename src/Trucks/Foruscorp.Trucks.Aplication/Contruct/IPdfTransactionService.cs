using Foruscorp.Trucks.Domain.Transactions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Contruct
{
    public interface IPdfTransactionService
    {
        Task<List<Transaction>> ParsePdfTransactionsAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<List<Transaction>> ParsePdfTransactionsFromStreamAsync(Stream pdfStream, CancellationToken cancellationToken = default);
        Task<List<Transaction>> ParsePdfTransactionsFromPathAsync(string pdfPath, CancellationToken cancellationToken = default);
    }
}

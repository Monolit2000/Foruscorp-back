using Microsoft.AspNetCore.Http;

namespace Foruscorp.FuelStations.Aplication.Contructs.Services
{
    public interface IExcelConverterService
    {
        Task<IFormFile> ConvertXlsToXlsxAsync(IFormFile xlsFile, CancellationToken cancellationToken = default);

        bool IsXlsFile(string fileName);
    }
}

using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;

namespace Foruscorp.FuelStations.Aplication.Contructs.Services
{
    public interface ILovesApiService
    {
        Task<LovesApiResponseModel?> GetStoresAsync(CancellationToken cancellationToken = default);
    }
}

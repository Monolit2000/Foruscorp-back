using Microsoft.AspNetCore.Http;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;

namespace Foruscorp.FuelStations.Aplication.Contructs.Services
{
    public interface IXMlFuelStationService
    {
        Task<List<XmlLoversStationParceModel>> ParceLoversExcelFile(IFormFile file, CancellationToken cancellationToken = default);
        Task<List<XmlTaAndPetroStationParceModel>> ParceTaAndPetroStationFile(IFormFile file, CancellationToken cancellationToken = default);

        Task<List<XmlTaAndPetroStationInfoModel>> ParceTaAndPetroStationInfoFile(IFormFile file, CancellationToken cancellationToken = default);
    }
}

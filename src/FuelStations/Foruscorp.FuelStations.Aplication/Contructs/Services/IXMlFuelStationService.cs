using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;

namespace Foruscorp.FuelStations.Aplication.Contructs.Services
{
    public interface IXMlFuelStationService
    {
        Task<List<XmlLoversStationParceModel>> ParceLoversExcelFile(IFormFile file, CancellationToken cancellationToken = default);
    }
}

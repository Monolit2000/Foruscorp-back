using Foruscorp.FuelStations.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct
{
    public interface IFuelStationService
    {
        public Task<FuelStationIntegrationDto> GetFuelStationInfoAsync(Guid fuelStationId, CancellationToken cancellationToken = default);
    }
}

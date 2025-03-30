using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public interface IFuelStationRepository
    {
        public Task<FuelStation> GetByIdAsync(Guid id);

        public Task AddAsync(FuelStation fuelStation);

        public Task UpdateAsync(FuelStation fuelStation);

        public Task DeleteAsync(Guid id);

        public Task<IEnumerable<FuelStation>> GetByRadiusAsync(GeoPoint coordinates, int radius);

        public Task<IEnumerable<FuelStation>> GetByFuelTypeAsync(FuelType fuelType);

        public Task<IEnumerable<FuelStation>> GetByFuelTypeAndRadiusAsync(FuelType fuelType, GeoPoint coordinates, int radius);

    }
}

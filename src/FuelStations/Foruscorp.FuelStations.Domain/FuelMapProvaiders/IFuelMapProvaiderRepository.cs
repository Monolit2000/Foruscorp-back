using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelMapProvaiders
{
    public interface IFuelMapProvaiderRepository
    {
        public Task Add(FuelMapProvaider fuelMapProvaider);

        public Task<FuelMapProvaider> GetById(Guid id);

        public Task<FuelMapProvaider> GetByApiToken(string apiToken);

        public Task<FuelMapProvaider> GetByName(string apiToken);

        public Task<IEnumerable<FuelMapProvaider>> GetAll();
    }
}

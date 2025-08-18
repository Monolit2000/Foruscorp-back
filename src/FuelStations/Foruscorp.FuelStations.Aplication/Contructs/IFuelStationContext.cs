using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Domain.FuelStations;
using Foruscorp.FuelStations.Domain.FuelMapProvaiders;

namespace Foruscorp.FuelStations.Aplication.Contructs
{
    public interface IFuelStationContext
    {
        public DbSet<FuelStation> FuelStations { get; set; }
        public DbSet<FuelMapProvaider> FuelMapProvaiders { get; set; }
        public DbSet<PriceLoadAttempt> PriceLoadAttempts { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

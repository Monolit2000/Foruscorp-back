using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.DriverFuelHistorys
{
    public class DriverFuelHistory
    {
        public Guid Id { get; private set; }
        public Guid DriverId { get; private set; }
        public decimal FuelAmount { get; private set; }
        public DateTime RecordedAt { get; private set; }
        public FuelStatus Status { get; private set; }

        private DriverFuelHistory() { }

        private DriverFuelHistory(Guid driverId, decimal fuelAmount)
        {
            Id = Guid.NewGuid();
            DriverId = driverId;
            FuelAmount = fuelAmount;
            RecordedAt = DateTime.UtcNow;
            Status = FuelStatus.Normal;
        }

        public static DriverFuelHistory CreateNew(Guid driverId, decimal fuelAmount)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException("Driver identifier cannot be empty.", nameof(driverId));
            if (fuelAmount < 0)
                throw new ArgumentException("Fuel amount cannot be negative.", nameof(fuelAmount));

            return new DriverFuelHistory(driverId, fuelAmount);
        }

        public void UpdateFuelStatus(FuelStatus newStatus)
        {
            Status = newStatus;
        }
    }

    public enum FuelStatus
    {
        Normal,
        Low,
        Critical
    }
}

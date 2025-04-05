using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks.Events;

namespace Foruscorp.Trucks.Domain.Trucks
{
    public class Truck : Entity, IAggregateRoot
    {
        public Guid Id { get; private set; }
        public string Ulid { get; private set; }
        public string LicensePlate { get; private set; }
        public TruckStatus Status { get; private set; }
        public Guid? DriverId { get; private set; }
        public Driver Driver { get; private set; }

        private Truck() { }

        private Truck(
            string ulid, 
            string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(ulid))
                throw new ArgumentException("ULID cannot be empty.", nameof(ulid));
            if (string.IsNullOrWhiteSpace(licensePlate))
                throw new ArgumentException("License plate cannot be empty.", nameof(licensePlate));

            Id = Guid.NewGuid();
            Ulid = ulid;
            LicensePlate = licensePlate;
            Status = TruckStatus.Inactive;
            DriverId = null;

            AddDomainEvent(new TruckCreatedEvent(this));    
        }

        public static Truck CreateNew(
            string ulid, 
            string licensePlate)
        {
            return new Truck(
                ulid, 
                licensePlate);
        }

        public void AttachDriver(Guid driverId)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException("Driver identifier cannot be empty.", nameof(driverId));
            if (Status != TruckStatus.Active)
                throw new InvalidOperationException("Truck must be active to attach a driver.");
            if (DriverId.HasValue)
                throw new InvalidOperationException("Truck already has an assigned driver.");

            DriverId = driverId;
        }

        public void DetachDriver()
        {
            if (!DriverId.HasValue)
                throw new InvalidOperationException("Truck does not have an assigned driver.");

            DriverId = null;
        }

        public void SetActiveStatus()
        {
            if (Status == TruckStatus.Active)
                throw new InvalidOperationException("Truck is already active.");

            Status = TruckStatus.Active;
        }

        public void SetInactiveStatus()
        {
            if (Status == TruckStatus.Active)
                throw new InvalidOperationException("Truck is already active.");

            Status = TruckStatus.Active;
        }

    }

    public enum TruckStatus
    {
        Active,
        Inactive
    }
}

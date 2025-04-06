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

        public void AttachDriver(Driver driver)
        {
            if (driver == null)
                throw new ArgumentException("Driver identifier cannot be empty.", nameof(driver.Id));
            //if (Status != TruckStatus.Active)
            //    throw new InvalidOperationException("Truck must be active to attach a driver.");
            if (Driver != null)
                throw new InvalidOperationException("Truck already has an assigned driver.");

            //DriverId = driver.Id;
            Driver = driver;

            AddDomainEvent(new TruckDriverAttachedDomainEvent(Id, Driver.Id));
        }

        public void DetachDriver()
        {
            if (DriverId == null)
                throw new InvalidOperationException("Truck does not have an assigned driver.");

            Driver = null;

            AddDomainEvent(new TruckDriverDetachedDomainEvent(Id, DriverId.Value));   
        }

        public void SetActiveStatus()
        {
            if (Status == TruckStatus.Active)
                throw new InvalidOperationException("Truck is already active.");

            Status = TruckStatus.Active;

            AddDomainEvent(new TruckActivatedEvent(Id, Ulid));
        }

        public void SetInactiveStatus()
        {
            if (Status == TruckStatus.Active)
                throw new InvalidOperationException("Truck is already active.");

            Status = TruckStatus.Active;

            AddDomainEvent(new TruckDeactivatedEvent(Id, Ulid));    
        }

    }

    public enum TruckStatus
    {
        Active,
        Inactive
    }
}

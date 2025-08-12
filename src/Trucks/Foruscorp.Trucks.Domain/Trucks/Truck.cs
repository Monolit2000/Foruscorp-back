using Foruscorp.BuildingBlocks.Domain;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks.Events;

namespace Foruscorp.Trucks.Domain.Trucks
{
    public class Truck : Entity, IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid? CompanyId { get; private set; } 
        public string ProviderTruckId { get; private set; }
        public string Name { get; set; }
        public string Vin { get; private set; }
        public string Serial { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public string HarshAccelerationSettingType { get; private set; }
        public string LicensePlate { get; private set; }
        public string Year { get; set; }
        public DateTime CreatedAtTime { get; private set; }
        public DateTime UpdatedAtTime { get; private set; }
        public TruckStatus Status { get; private set; }
        public Guid? DriverId { get; private set; }
        public Driver Driver { get; private set; }

        private Truck() { }

        private Truck(
             string ulid,
             string name,
             string providerTruckId,
             string vin,
             string serial,
             string make,
             string model,
             string harshAccelerationSettingType,
             string licensePlate,
             string year,
             DateTime createdAtTime,
             DateTime updatedAtTime)
        {
            if (string.IsNullOrWhiteSpace(ulid))
                throw new ArgumentException("ULID cannot be empty.", nameof(ulid));
            if (string.IsNullOrWhiteSpace(providerTruckId))
                throw new ArgumentException("ProviderTruckId cannot be empty.", nameof(providerTruckId));
            if (string.IsNullOrWhiteSpace(vin))
                throw new ArgumentException("VIN cannot be empty.", nameof(vin));
            //if (string.IsNullOrWhiteSpace(serial))
            //    throw new ArgumentException("Serial cannot be empty.", nameof(serial));
            if (string.IsNullOrWhiteSpace(make))
                throw new ArgumentException("Make cannot be empty.", nameof(make));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be empty.", nameof(model));
            //if (string.IsNullOrWhiteSpace(licensePlate))
            //    throw new ArgumentException("License plate cannot be empty.", nameof(licensePlate));

            Id = Guid.NewGuid();
            Name = name;
            ProviderTruckId = providerTruckId;
            Vin = vin;
            Serial = serial;
            Make = make;
            Model = model;
            HarshAccelerationSettingType = harshAccelerationSettingType; // Optional, can be null
            LicensePlate = licensePlate;
            Year = year;
            CreatedAtTime = createdAtTime;
            UpdatedAtTime = updatedAtTime;
        
            Status = TruckStatus.Inactive;

            AddDomainEvent(new TruckCreatedEvent(this));
        }

        public static Truck CreateNew(
            string ulid,
            string name,
            string providerTruckId,
            string vin,
            string serial,
            string make,
            string model,
            string harshAccelerationSettingType,
            string licensePlate,
            string year,
            DateTime createdAtTime,
            DateTime updatedAtTime)
        {
            return new Truck(
                ulid,
                name,
                providerTruckId,
                vin,
                serial,
                make,
                model,
                harshAccelerationSettingType,
                licensePlate,
                year,
                createdAtTime,
                updatedAtTime);
        }

        public void Update(
            string vin,
            string serial,
            string make,
            string model,
            string harshAccelerationSettingType,
            string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(vin))
                throw new ArgumentException("VIN cannot be empty.", nameof(vin));
            if (string.IsNullOrWhiteSpace(serial))
                throw new ArgumentException("Serial cannot be empty.", nameof(serial));
            if (string.IsNullOrWhiteSpace(make))
                throw new ArgumentException("Make cannot be empty.", nameof(make));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be empty.", nameof(model));
            if (string.IsNullOrWhiteSpace(licensePlate))
                throw new ArgumentException("License plate cannot be empty.", nameof(licensePlate));

            Vin = vin;
            Serial = serial;
            Make = make;
            Model = model;
            HarshAccelerationSettingType = harshAccelerationSettingType;
            LicensePlate = licensePlate;
            UpdatedAtTime = DateTime.UtcNow;

            //AddDomainEvent(new TruckUpdatedEvent(this));
        }

        public void AttachDriver(Driver driver)
        {
            if (driver == null)
                throw new ArgumentException("Driver identifier cannot be empty.", nameof(driver.Id));
            //if (Status != TruckStatus.Active)
            //    throw new InvalidOperationException("Truck must be active to attach a driver.");
            if (Driver != null)
                throw new InvalidOperationException("Truck already has an assigned driver.");

            DriverId = driver.Id;
            Driver = driver;

            AddDomainEvent(new TruckDriverAttachedDomainEvent(Id, Driver.Id));
        }

        public void DetachDriver()
        {
            if (DriverId == null)
                return;

            Driver = null;

            AddDomainEvent(new TruckDriverDetachedDomainEvent(Id, DriverId.Value));   
        }

        public void SetActiveStatus()
        {
            if (Status == TruckStatus.Active)
                return;
            //throw new InvalidOperationException("Truck is already active.");
            Status = TruckStatus.Active;

            AddDomainEvent(new TruckActivatedEvent(Id));
        }

        public void SetCompany(Guid companyId)
        {
            CompanyId = companyId;
        }   

        public void SetInactiveStatus()
        {
            if (Status == TruckStatus.Active)
                return;
            //throw new InvalidOperationException("Truck is already active.");

            Status = TruckStatus.Active;

            AddDomainEvent(new TruckDeactivatedEvent(Id));    
        }

        public void SetFreeStatus()
        {
            if (Status == TruckStatus.Free)
                return;
            //throw new InvalidOperationException("Truck is already free.");

            Status = TruckStatus.Free;
            // Add any additional logic for setting the truck to free status
        }   

        public void UpdateStatus(TruckStatus status)
        {
            if (Status == status)
                return;
            Status = status;
        }

    }

    public enum TruckStatus
    {
        Active,
        Inactive,
        Free
    }
}

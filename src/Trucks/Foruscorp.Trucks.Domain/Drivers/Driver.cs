﻿

using Foruscorp.BuildingBlocks.Domain;
using Foruscorp.Trucks.Domain.DriverFuelHistorys;
using Foruscorp.Trucks.Domain.Drivers.Events;
using Foruscorp.Trucks.Domain.RouteOffers;
using Foruscorp.Trucks.Domain.Trucks;

namespace Foruscorp.Trucks.Domain.Drivers
{
    public class Driver : Entity, IAggregateRoot
    {
        public Guid? TruckId { get; private set; }
        public Truck Truck { get; private set; } 

        public readonly List<DriverBonus> Bonuses = [];

        public readonly List<DriverFuelHistory> FuelHistories = [];

        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public string LicenseNumber { get; private set; }
        public DriverStatus Status { get; private set; }
        public DateTime HireDate { get; private set; }
        public int ExperienceYears { get; private set; }
        public decimal Bonus { get; private set; } 


        public decimal TotalBonus => Bonuses.Sum(b => b.Amount);

        private Driver() { }

        private Driver(string fullName)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
        }


        public static Driver CreateNew(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
            return new Driver(fullName);
        }

        //private Driver(
        //    string fullName, 
        //    string licenseNumber,
        //    DateTime hireDate, 
        //    int experienceYears)
        //{
        //    if (string.IsNullOrWhiteSpace(fullName))
        //        throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        //    if (string.IsNullOrWhiteSpace(licenseNumber))
        //        throw new ArgumentException("License number cannot be empty.", nameof(licenseNumber));
        //    if (experienceYears < 0)
        //        throw new ArgumentException("Experience years cannot be negative.", nameof(experienceYears));

        //    Id = Guid.NewGuid();
        //    FullName = fullName;
        //    LicenseNumber = licenseNumber;
        //    Status = DriverStatus.Active;
        //    HireDate = hireDate;
        //    ExperienceYears = experienceYears;
        //    Bonus = 0;
        //    TruckId = null;
        //}

        //public static Driver CreateNew(
        //    string fullName,
        //    string licenseNumber, 
        //    DateTime hireDate, 
        //    int experienceYears) 
        //    => new Driver(
        //        fullName, 
        //        licenseNumber,
        //        hireDate, 
        //        experienceYears);



        public RouteOffer ProposeRouteOffer(string description)
        {
            if (Status != DriverStatus.Active)
                throw new InvalidOperationException("Only active drivers can create route offers.");
            if (!TruckId.HasValue)
                throw new InvalidOperationException("Driver must be assigned to a truck to create a route offer.");

            return RouteOffer.CreateNew(this.Id, description);
        }

  
        public void AssignToTruck(Guid truckId)
        {
            if (truckId == Guid.Empty)
                throw new ArgumentException("Truck identifier cannot be empty.", nameof(truckId));
            if (Status != DriverStatus.Active)
                throw new InvalidOperationException("Only active drivers can be assigned to a truck.");
            if (TruckId.HasValue)
                throw new InvalidOperationException("Driver is already assigned to a truck.");

            TruckId = truckId;
        }

        public void ReleaseFromTruck()
        {
            if (!TruckId.HasValue)
                throw new InvalidOperationException("Driver is not assigned to any truck.");

            TruckId = null;
        }

 
        public void IncreaseBonus(decimal amount, string reason)
        {
            var bonus = DriverBonus.CreateNew(this.Id, amount, reason);
            Bonuses.Add(bonus);

            AddDomainEvent(new DriverBonusAddedDomainEvent(this.Id, amount, reason));   
        }

        public void DecreaseBonus(decimal amount, string reason)
        {
            if (TotalBonus - amount < 0)
                throw new InvalidOperationException("Total bonus cannot be negative.");

            var negativeBonus = DriverBonus.CreateNew(this.Id, -amount, reason);
            Bonuses.Add(negativeBonus);

            AddDomainEvent(new DriverBonusDecreasedDomainEvent(this.Id, -amount, reason));  
        }

        public void Deactivate()
        {
            if (Status == DriverStatus.Suspended)
                throw new InvalidOperationException("Driver is already suspended.");
            if (TruckId.HasValue)
                throw new InvalidOperationException("Cannot suspend a driver assigned to a truck.");

            Status = DriverStatus.Suspended;
        }

   
        public void Reactivate()
        {
            if (Status != DriverStatus.Suspended)
                throw new InvalidOperationException("Driver is not suspended.");

            Status = DriverStatus.Active;
        }


        public void AddFuelHistory(decimal fuelAmount)
        {
            if (fuelAmount < 0)
                throw new ArgumentException("Fuel amount cannot be negative.", nameof(fuelAmount));

            var fuelHistory = DriverFuelHistory.CreateNew(this.Id, fuelAmount);
            FuelHistories.Add(fuelHistory);
        }


        public void UpdateFuelStatus(Guid fuelHistoryId, FuelStatus newStatus)
        {
            var fuelHistory = FuelHistories.FirstOrDefault(fh => fh.Id == fuelHistoryId);
            if (fuelHistory == null)
                throw new InvalidOperationException("Fuel history record not found.");

            fuelHistory.UpdateFuelStatus(newStatus);
        }
    }

    public enum DriverStatus
    {
        Active,
        Suspended
    }
}


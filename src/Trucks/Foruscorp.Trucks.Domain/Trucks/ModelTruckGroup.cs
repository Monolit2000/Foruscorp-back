using Foruscorp.BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Trucks
{
    public class ModelTruckGroup : Entity, IAggregateRoot
    {
        public List<Truck> Trucks { get; private set; } = [];

        public Guid Id { get; private set; }
        public string TruckGrouName { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public string Year { get; private set; }
        public double AverageFuelConsumption { get; private set; } // in liters per 100km
        public double AveregeWeight { get; private set; } 
        public double FuelCapacity { get; private set; } 

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public ModelTruckGroup(string make, string model, string year)
        {
            Id = Guid.NewGuid();
            Make = make;
            Model = model;
            Year = year;

            TruckGrouName = $"{make} {model} {year}";

            FuelCapacity = 200.0; //gl

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAverageFuelConsumption(double averageFuelConsumption)
        {
            if (averageFuelConsumption <= 0)
                throw new ArgumentException("Average fuel consumption must be greater than zero.", nameof(averageFuelConsumption));
            AverageFuelConsumption = averageFuelConsumption;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAveregeWeight(double averageWeight)
        {
            if (averageWeight <= 0)
                throw new ArgumentException("Average weight must be greater than zero.", nameof(averageWeight));
            AveregeWeight = averageWeight;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetFuelCapacity(double fuelCapacity)
        {
            if (fuelCapacity <= 0)
                throw new ArgumentException("Fuel capacity must be greater than zero.", nameof(fuelCapacity));
            FuelCapacity = fuelCapacity;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

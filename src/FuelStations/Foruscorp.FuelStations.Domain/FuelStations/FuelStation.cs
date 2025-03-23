using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public class FuelStation
    {
        private readonly List<FuelPrice> _fuelPrices = new();

        public Guid Id { get; private set; }
        public string Address { get; private set; }
        public GeoPoint Coordinates { get; private set; }
        public IReadOnlyList<FuelPrice> FuelPrices => _fuelPrices.AsReadOnly();
        public DateTime LastUpdated { get; private set; }

        private FuelStation() { }

        private FuelStation(string address, GeoPoint coordinates)
        {
            Id = Guid.NewGuid();
            Address = address;
            Coordinates = coordinates;
            LastUpdated = DateTime.UtcNow;
        }

        public static FuelStation CreateNew(string address, GeoPoint coordinates)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be empty", nameof(address));

            return new FuelStation(address, coordinates);
        }

        public void UpdateFuelPrices(IEnumerable<FuelPrice> newPrices)
        {
            if (newPrices == null || !newPrices.Any())
                throw new ArgumentException("Fuel prices collection cannot be null or empty", nameof(newPrices));

            foreach (var price in newPrices)
                ValidateFuelPrice(price);

            _fuelPrices.Clear();
            _fuelPrices.AddRange(newPrices);
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateFuelPrice(FuelType fuelType, decimal price, decimal? discountedPrice = null)
        {
            var newFuelPrice = new FuelPrice(fuelType, price, discountedPrice);

            ValidateFuelPrice(newFuelPrice);

            var existingPrice = _fuelPrices.FirstOrDefault(fp => fp.FuelType == fuelType);
            if (existingPrice != null)
                _fuelPrices.Remove(existingPrice);

            _fuelPrices.Add(newFuelPrice);
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateLocation(string newAddress, GeoPoint newCoordinates)
        {
            if (string.IsNullOrWhiteSpace(newAddress))
                throw new ArgumentException("Address cannot be empty", nameof(newAddress));

            Address = newAddress;
            Coordinates = newCoordinates;
            LastUpdated = DateTime.UtcNow;
        }

        public decimal CalculateAverageFuelPrice(bool useDiscountedPrice = false)
        {
            if (!_fuelPrices.Any())
                return 0;

            if (useDiscountedPrice)
            {
                var discountedPrices = _fuelPrices
                    .Where(fp => fp.DiscountedPrice.HasValue)
                    .Select(fp => fp.DiscountedPrice.Value);
                return discountedPrices.Any() ? discountedPrices.Average() : 0;
            }

            return _fuelPrices.Average(fp => fp.Price);
        }

        public decimal GetPriceForFuelType(FuelType fuelType, bool useDiscountedPrice = false)
        {
            var fuelPrice = _fuelPrices.FirstOrDefault(fp => fp.FuelType == fuelType)
                ?? throw new InvalidOperationException($"Price for fuel type {fuelType.Name} not found");

            return useDiscountedPrice && fuelPrice.DiscountedPrice.HasValue
                ? fuelPrice.DiscountedPrice.Value
                : fuelPrice.Price;
        }

        private void ValidateFuelPrice(FuelPrice fuelPrice)
        {
            if (fuelPrice.FuelType == null)
                throw new ArgumentException("Fuel type cannot be null", nameof(fuelPrice.FuelType));
            if (_fuelPrices.Any(fp => fp.FuelType == fuelPrice.FuelType && fp != fuelPrice))
                throw new InvalidOperationException($"Duplicate fuel type {fuelPrice.FuelType.Name} detected");
        }
    }
}
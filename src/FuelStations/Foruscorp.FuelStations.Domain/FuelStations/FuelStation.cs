

using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public class FuelStation : Entity, IAggregateRoot
    {
        public readonly List<FuelPrice> FuelPrices = [];

        public Guid Id { get; private set; }
        public string ProviderName { get; private set; } 
        public string Address { get; private set; }
        public string FuelProvider { get; private set; }    
        public GeoPoint Coordinates { get; private set; }
        public DateTime LastUpdated { get; private set; }

        private FuelStation() { }

        private FuelStation(string address, string providerName, GeoPoint coordinates, List<FuelPrice> fuelPrices = null)
        {
            Id = Guid.NewGuid();
            ProviderName = providerName;
            Address = address;
            Coordinates = coordinates;
            LastUpdated = DateTime.UtcNow;

            if (fuelPrices != null && fuelPrices.Any())
            {
                FuelPrices = fuelPrices;
            }
        }

        public static FuelStation CreateNew(string address, string providerName, GeoPoint coordinates, List<FuelPrice> fuelPrices = null)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be empty", nameof(address));

            return new FuelStation(address, providerName, coordinates, fuelPrices);
        }

        public void UpdateFuelPrices(IEnumerable<FuelPrice> newPrices)
        {
            if (newPrices == null || !newPrices.Any())
                throw new ArgumentException("Fuel prices collection cannot be null or empty", nameof(newPrices));

            foreach (var price in newPrices)
                ValidateFuelPrice(price);

            FuelPrices.Clear();
            FuelPrices.AddRange(newPrices);
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateFuelPrice(FuelType fuelType, double price, double? discountedPrice = null)
        {
            var newFuelPrice = new FuelPrice(fuelType, price, discountedPrice);

            ValidateFuelPrice(newFuelPrice);

            var existingPrice = FuelPrices.FirstOrDefault(fp => fp.FuelType == fuelType);
            if (existingPrice != null)
                FuelPrices.Remove(existingPrice);

            FuelPrices.Add(newFuelPrice);
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

        public double CalculateAverageFuelPrice(bool useDiscountedPrice = false)
        {
            if (!FuelPrices.Any())
                return 0;

            if (useDiscountedPrice)
            {
                var discountedPrices = FuelPrices
                    .Where(fp => fp.DiscountedPrice.HasValue)
                    .Select(fp => fp.DiscountedPrice.Value);
                return discountedPrices.Any() ? discountedPrices.Average() : 0;
            }

            return FuelPrices.Average(fp => fp.Price);
        }

        public double GetPriceForFuelType(FuelType fuelType, bool useDiscountedPrice = false)
        {
            var fuelPrice = FuelPrices.FirstOrDefault(fp => fp.FuelType == fuelType)
                ?? throw new InvalidOperationException($"Price for fuel type {fuelType.Name} not found");

            return useDiscountedPrice && fuelPrice.DiscountedPrice.HasValue
                ? fuelPrice.DiscountedPrice.Value
                : fuelPrice.Price;
        }

        private void ValidateFuelPrice(FuelPrice fuelPrice)
        {
            if (fuelPrice.FuelType == null)
                throw new ArgumentException("Fuel type cannot be null", nameof(fuelPrice.FuelType));
            if (FuelPrices.Any(fp => fp.FuelType == fuelPrice.FuelType && fp != fuelPrice))
                throw new InvalidOperationException($"Duplicate fuel type {fuelPrice.FuelType.Name} detected");
        }
    }

    public static class GeoCalculator
    {
        public static bool IsPointWithinRadius(
            GeoPoint center,
            GeoPoint point,
            double radiusKm)
        {
            if (radiusKm <= 0)
                throw new ArgumentException("Radius must be positive", nameof(radiusKm));

            return CalculateHaversineDistance(center, point) <= radiusKm;
        }

        public static double CalculateHaversineDistance(GeoPoint point1, GeoPoint point2)
        {
            const double EarthRadiusKm = 6371;

            var dLat = ToRadians(point2.Latitude - point1.Latitude);
            var dLon = ToRadians(point2.Longitude - point1.Longitude);

            var a = (double)Math.Sin((double)(dLat / 2)) * (double)Math.Sin((double)(dLat / 2)) +
                    (double)Math.Cos((double)ToRadians(point1.Latitude)) * (double)Math.Cos((double)ToRadians(point2.Latitude)) *
                    (double)Math.Sin((double)(dLon / 2)) * (double)Math.Sin((double)(dLon / 2));

            var c = 2 * (double)Math.Atan2(Math.Sqrt((double)a), Math.Sqrt(1 - (double)a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return (double)(degrees * (double)Math.PI / 180);
        }
    }
}


using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.FuelStations.Domain.FuelStations
{

    public enum SystemFuelProvider
    {
        Unknown = 0,
        TaPetro = 1,
        Loves = 2,
    }

    public class FuelStation : Entity, IAggregateRoot
    {
        public List<FuelPrice> FuelPrices = [];

        public Guid Id { get; private set; }
        public string FuelStationProviderId { get; set; }
        public string ProviderName { get; set; } 
        public string Address { get; private set; }
        public SystemFuelProvider SystemFuelProvider  { get; set; }
        public string FuelProvider { get; private set; }    
        public GeoPoint Coordinates { get; private set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }

        private FuelStation() { }

        private FuelStation(string address, string providerName, GeoPoint coordinates, List<FuelPrice> fuelPrices = null)
        {
            Id = Guid.NewGuid();
            ProviderName = providerName;
            Address = address;
            Coordinates = coordinates;
            LastUpdated = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
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

        //private static double ToRadians(double degrees)
        //{
        //    return (double)(degrees * (double)Math.PI / 180);
        //}

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        // <summary>
        /// Вычисляет кратчайшее расстояние (в км) от точки p до отрезка AB (каждый задан GeoPoint).
        /// По сути, проекция точки на бесконечную прямую, затем проверка, попадает ли проекция внутрь отрезка.
        /// Если проекция за пределами AB, берётся расстояние до ближайшей вершины A или B.
        /// </summary>
        public static double DistanceFromPointToSegmentKm(GeoPoint p, GeoPoint a, GeoPoint b)
        {
            // Перевод координат в радианы для более точного результата, но здесь допустимо работать в градусах,
            // т.к. потом мы всё равно используем Haversine для финального расстояния до проекции.
            // Однако, для параметра t (позиции вдоль AB) достаточно обычных градусных разниц.

            double lat1 = a.Latitude;
            double lon1 = a.Longitude;
            double lat2 = b.Latitude;
            double lon2 = b.Longitude;
            double lat3 = p.Latitude;
            double lon3 = p.Longitude;

            double dx = lat2 - lat1;
            double dy = lon2 - lon1;

            if (dx == 0 && dy == 0)
            {
                // A и B совпадают — возвращаем просто расстояние A↔P
                return CalculateHaversineDistance(p, a);
            }

            // Коэффициент проекции точки P на бесконечную прямую AB (в пределах [0,1] – внутри отрезка)
            double t = ((lat3 - lat1) * dx + (lon3 - lon1) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0.0, Math.Min(1.0, t)); // ограничиваем в [0,1]

            // Координаты «точки проекции» внутри отрезка AB
            double projLat = lat1 + t * dx;
            double projLon = lon1 + t * dy;
            var projPoint = new GeoPoint(projLat, projLon);

            // Возвращаем расстояние Гаверсина от P до проекции
            return CalculateHaversineDistance(p, projPoint);
        }

        /// <summary>
        /// Вычисляет «километровую позицию» вдоль отрезка AB, куда проецируется точка p. 
        /// То есть, если точка (или её проекция) лежит внутри сегмента AB, возвращает расстояние (в км) от A до этой проекции.
        /// Если проекция упала за пределы отрезка AB, то возвращает 0 (для A) или длину всего отрезка AB (для B).
        /// Этот метод нужен, чтобы понять, на каком «километре вдоль маршрута» находится станция.
        /// </summary>
        public static double DistanceAlongSegment(GeoPoint a, GeoPoint b, GeoPoint p)
        {
            double lat1 = a.Latitude;
            double lon1 = a.Longitude;
            double lat2 = b.Latitude;
            double lon2 = b.Longitude;
            double lat3 = p.Latitude;
            double lon3 = p.Longitude;

            double dx = lat2 - lat1;
            double dy = lon2 - lon1;

            if (dx == 0 && dy == 0)
            {
                // A и B совпадают
                return 0.0;
            }

            // Коэффициент проекции P на бесконечную прямую AB
            double t = ((lat3 - lat1) * dx + (lon3 - lon1) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0.0, Math.Min(1.0, t));

            // Точка проекции внутри отрезка AB
            double projLat = lat1 + t * dx;
            double projLon = lon1 + t * dy;
            var projPoint = new GeoPoint(projLat, projLon);

            // Возвращаем длину отрезка A→проекции (в километрах)
            return CalculateHaversineDistance(a, projPoint);
        }

     
    }
}
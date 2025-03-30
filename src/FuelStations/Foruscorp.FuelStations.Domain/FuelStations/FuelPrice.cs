using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public record FuelPrice
    {
        public FuelType FuelType { get; }
        public decimal Price { get; }
        public decimal? DiscountedPrice { get; } 
        public decimal PriceDifference => DiscountedPrice.HasValue ? Price - DiscountedPrice.Value : 0; 

        public FuelPrice(FuelType fuelType, decimal price, decimal? discountedPrice = null)
        {
            if (fuelType == null)
                throw new ArgumentNullException(nameof(fuelType), "Fuel type cannot be null");
            if (price < 0)
                throw new ArgumentException("Fuel price cannot be negative", nameof(price));
            if (discountedPrice.HasValue && discountedPrice < 0)
                throw new ArgumentException("Discounted price cannot be negative", nameof(discountedPrice));
            if (discountedPrice.HasValue && discountedPrice > price)
                throw new ArgumentException("Discounted price cannot be higher than regular price", nameof(discountedPrice));

            FuelType = fuelType;
            Price = price;
            DiscountedPrice = discountedPrice;
        }
    }
}

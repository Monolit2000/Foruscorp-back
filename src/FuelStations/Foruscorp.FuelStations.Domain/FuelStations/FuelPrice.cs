using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public record FuelPrice
    {
        public FuelType FuelType { get; set; }
        public double Price { get; set; }
        public double? DiscountedPrice { get; set; } 
        public double PriceDifference => DiscountedPrice.HasValue ? Price - DiscountedPrice.Value : 0; 
        public double PriceAfterDiscount => DiscountedPrice.HasValue ? Price - DiscountedPrice.Value : Price; 
        public double PriceDifferencePercentage => DiscountedPrice.HasValue ? (PriceDifference / Price) * 100 : 0;
        public double PriceDifferencePercentageWithDiscount => DiscountedPrice.HasValue ? (PriceDifference / DiscountedPrice.Value) * 100 : 0;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }



        public FuelPrice(FuelType fuelType, double price, double? discountedPrice = null)
        {
            if (fuelType == null)
                throw new ArgumentNullException(nameof(fuelType), "Fuel type cannot be null");
            if (price < 0)
                throw new ArgumentException("Fuel price cannot be negative", nameof(price));
            if (discountedPrice.HasValue && discountedPrice < 0)
                throw new ArgumentException("Discounted price cannot be negative", nameof(discountedPrice));
            //if (discountedPrice.HasValue && discountedPrice > price)
            //    throw new ArgumentException("Discounted price cannot be higher than regular price", nameof(discountedPrice));

            FuelType = fuelType;
            Price = price;
            DiscountedPrice = discountedPrice;
            CreatedAt = DateTime.UtcNow;
        }
    }
}

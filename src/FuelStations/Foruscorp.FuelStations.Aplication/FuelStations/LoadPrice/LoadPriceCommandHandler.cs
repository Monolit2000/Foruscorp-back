using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadPrice
{
    public record LoadPriceCommand(IFormFile file) : IRequest<Result>;
    public class LoadPriceCommandHandler(
        IFuelStationContext fuelStationContext,
        IXMlFuelStationService xMlFuelStationService) : IRequestHandler<LoadPriceCommand, Result>
    {
        public async Task<Result> Handle(LoadPriceCommand request, CancellationToken cancellationToken)
        {
            if (request.file == null || request.file.Length == 0)
            {
                return Result.Fail("File is empty or not provided.");
            }

            var models = await xMlFuelStationService.ParceTaAndPetroStationFile(request.file, cancellationToken);

            var fuelStations = await fuelStationContext
                .FuelStations
                .Include(s => s.FuelPrices)
                .ToListAsync(cancellationToken);

            foreach (var station in fuelStations)
            {
                var match = models.FirstOrDefault(m => m.Id == station.FuelStationProviderId);

                if (match is not null)
                {
                    var price = station.FuelPrices.FirstOrDefault();
                    if (price is not null)
                    {
                        price.FuelType = FuelType.Gasoline95;
                        price.Price = match.Price;
                        price.DiscountedPrice = match.Discount;
                        price.UpdatedAt = DateTime.UtcNow;  
                    }
                    else
                    {
                        station.FuelPrices.Add(new FuelPrice(
                            FuelType.Gasoline95,
                            match.Price,
                            match.Discount));
                    }
                }
            }

            await fuelStationContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();

        }
    }
}

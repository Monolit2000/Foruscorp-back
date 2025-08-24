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

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadLoversPrice
{
    public record LoadLoversPriceCommand(IFormFile file) : IRequest<Result>;
    
    public class LoadLoversPriceCommandHandler(
        IFuelStationContext fuelStationContext,
        IXMlFuelStationService xMlFuelStationService) : IRequestHandler<LoadLoversPriceCommand, Result>
    {
        public async Task<Result> Handle(LoadLoversPriceCommand request, CancellationToken cancellationToken)
        {
            if (request.file == null || request.file.Length == 0)
            {
                return Result.Fail("File is empty or not provided.");
            }

            var models = await xMlFuelStationService.ParceLoversExcelFile(request.file, cancellationToken);

            var fuelStations = await fuelStationContext
                .FuelStations
                .Include(s => s.FuelPrices)
                .ToListAsync(cancellationToken);

            foreach (var station in fuelStations)
            {
                var match = models.FirstOrDefault(m => m.Id == station.FuelStationProviderId && station.SystemFuelProvider == SystemProvider.Loves);

                if (match is not null)
                {
                    var price = station.FuelPrices.FirstOrDefault();
                    if (price is not null)
                    {
                        price.FuelType = FuelType.Gasoline95;
                        price.Price = (double)match.PumpPrice;
                        price.DiscountedPrice = (double)match.Discount;
                        price.UpdatedAt = DateTime.UtcNow;

                        fuelStationContext.FuelStations.Update(station);
                    }
                    else
                    {
                        station.FuelPrices.Add(new FuelPrice(
                            FuelType.Gasoline95,
                            (double)match.PumpPrice,
                            (double)match.Discount));

                        fuelStationContext.FuelStations.Update(station);
                    }
                }
            }

            await fuelStationContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}

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

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadTaAndPetroPrice
{
    public record LoadTaAndPetroPriceCommand(IFormFile file) : IRequest<Result>;
    public class LoadTaAndPetroPriceCommandHandler(
        IFuelStationContext fuelStationContext,
        IXMlFuelStationService xMlFuelStationService) : IRequestHandler<LoadTaAndPetroPriceCommand, Result>
    {
        public async Task<Result> Handle(LoadTaAndPetroPriceCommand request, CancellationToken cancellationToken)
        {
            if (request.file == null || request.file.Length == 0)
            {
                return Result.Fail("File is empty or not provided.");
            }

            var models = await xMlFuelStationService.ParceTaAndPetroStationFile(request.file, cancellationToken);

            var fuelStations = await fuelStationContext
                .FuelStations
                .Where(s => s.SystemFuelProvider == SystemProvider.TaPetro)
                .Include(s => s.FuelPrices)
                .ToListAsync(cancellationToken);

            var updatedStations = new List<FuelStation>();
            var notUpdatedStations = new List<FuelStation>();

            foreach (var station in fuelStations)
            {
                var match = models.FirstOrDefault(m => m.Id == station.FuelStationProviderId && station.SystemFuelProvider == SystemProvider.TaPetro);

                if (match is not null)
                {
                    var price = station.FuelPrices.FirstOrDefault();
                    if (price is not null)
                    {
                        price.FuelType = FuelType.Gasoline95;
                        price.Price = match.Price;
                        price.DiscountedPrice = match.Discount;
                        price.UpdatedAt = DateTime.UtcNow;

                        fuelStationContext.FuelStations.Update(station);
                        updatedStations.Add(station);
                    }
                    else
                    {
                        station.FuelPrices.Add(new FuelPrice(
                            FuelType.Gasoline95,
                            match.Price,
                            match.Discount));

                        fuelStationContext.FuelStations.Update(station);
                    }
                }
                else
                {
                    notUpdatedStations.Add(station);
                }
            }

            await fuelStationContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();

        }
    }
}

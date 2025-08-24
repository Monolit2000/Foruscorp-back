using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadInfo
{

    public record LoadInfoCommand(IFormFile file) : IRequest<Result>;

    public class LoadInfoCommandHandler(
        IXMlFuelStationService xMlFuelStationService,
        IFuelStationContext fuelStationContext) : IRequestHandler<LoadInfoCommand, Result>
    {
        public async Task<Result> Handle(LoadInfoCommand request, CancellationToken cancellationToken)
        {
            if (request.file == null || request.file.Length == 0)
            {
                return Result.Fail("File is empty or not provided.");
            }

            List<XmlTaAndPetroStationInfoModel> models = await xMlFuelStationService.ParceTaAndPetroStationInfoFile(request.file, cancellationToken);

            var fuelStations = await fuelStationContext.FuelStations.ToListAsync(cancellationToken);

            var newStationsList = new List<FuelStation>();

            foreach (var model in models)
            {
                var fuelStation = fuelStations.FirstOrDefault(s =>
                    Math.Round(s.Coordinates.Longitude, 3) == Math.Round(model.Longitude, 3) &&
                    Math.Round(s.Coordinates.Latitude, 3) == Math.Round(model.Latitude, 3) ||
                        (s.FuelStationProviderId != null && s.FuelStationProviderId == model.Id));

                if (fuelStation is not null && (fuelStation.SystemFuelProvider == SystemFuelProvider.TaPetro || fuelStation.SystemFuelProvider == SystemFuelProvider.Unknown))
                {
                    fuelStation.FuelStationProviderId = model.Id;
                    fuelStation.SystemFuelProvider = SystemFuelProvider.TaPetro;
                    fuelStationContext.FuelStations.Update(fuelStation);
                }
                else
                {

                    var newFuelStation = FuelStation.CreateNew(
                            model.Directions + " " + model.Address,
                            model.Brand,
                            new GeoPoint(
                                model.Latitude,
                                model.Longitude)
                            );
                    newFuelStation.FuelStationProviderId = model.Id;
                    newFuelStation.SystemFuelProvider = SystemFuelProvider.TaPetro;
                    newStationsList.Add(newFuelStation);
                }
            }

            await fuelStationContext.FuelStations.AddRangeAsync(newStationsList);
            await fuelStationContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}

using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.RouteValidation
{
    public class ValidateRouteCommand : IRequest<Result>
    {
        public Guid RouteSectionId { get; set; }

        public List<FuelStationCorrectDto> fuelStationCorrectDtos { get; set; } = [];


    }
    public class ValidateRouteCommandHandler(
        IFuelRouteContext fuelRouteContext) : IRequestHandler<ValidateRouteCommand, Result>
    {
        public Task<Result> Handle(ValidateRouteCommand request, CancellationToken cancellationToken)
        {
            var routeSection = fuelRouteContext.RouteSections
                .Include(fs => fs.FuelRouteStations)
                .FirstOrDefault(rs => rs.Id == request.RouteSectionId);

            var points = routeSection.FuelRouteStations
                .Where(fs => fs.IsAlgorithm)
                .Select(fs => new FuelStationCorrectDto
                {
                    FuelStationId = fs.FuelStationId,
                    Refill = double.Parse(fs.Refill, System.Globalization.CultureInfo.InvariantCulture)
                }).ToList();


            var mappedPoints = ExtractRoutePoints(PolylineEncoder.DecodePolyline(routeSection.EncodeRoute));

            throw new NotImplementedException();
        }

        public List<GeoPoint> ExtractRoutePoints(List<List<double>> points)
        {
            var routePoints = points.Select(p => new GeoPoint(p[0], p[1]))
                .ToList();

            return routePoints;
        }

    }



    public class FuelStationCorrectDto
    {
        public Guid FuelStationId { get; set; }
        public double Refill { get; set; }
    }
}

using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using static System.Net.WebRequestMethods;

namespace Foruscorp.TrucksTracking.Aplication.FuelStations.CheckNearFuelStation
{
    public record CheckNearFuelStationCommand(Guid TruckId, double TruckLongitude, double TruckLatitude, Guid? RouteId) : IRequest;

    public class CheckNearFuelStationCommandHandler(
        IPublishEndpoint publishEndpoint,
        ITruckTrackingContext truckTrackingContext) : IRequestHandler<CheckNearFuelStationCommand>
    {
        public async Task Handle(CheckNearFuelStationCommand request, CancellationToken cancellationToken)
        {
            if (!request.RouteId.HasValue)
                return;

            var fuelStationPlans = await truckTrackingContext.NearFuelStationPlans
                .Where(fsp => fsp.TruckId == request.TruckId && !fsp.IsProcessed && fsp.RouteId == request.RouteId)
                .ToListAsync(cancellationToken);

            foreach (var fuelStationPlan in fuelStationPlans)
            {
                if (GeoUtils.CalculateDistance(
                    fuelStationPlan.Latitude, fuelStationPlan.Longitude, request.TruckLatitude, request.TruckLongitude) > fuelStationPlan.NearDistance)
                    return;

                var truckTracker = await truckTrackingContext.TruckTrackers
                    .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId, cancellationToken);

                fuelStationPlan.MarkIsNear(truckTracker.CurrentTruckLocation);

                truckTrackingContext.NearFuelStationPlans.Update(fuelStationPlan);

                await truckTrackingContext.SaveChangesAsync(cancellationToken);

                fuelStationPlan.Address ??= "N/A";

                await PublishEvent(
                    request.TruckId,
                    fuelStationPlan.FuelStationId,
                    fuelStationPlan.Address,
                    fuelStationPlan.Longitude,
                    fuelStationPlan.Latitude,
                    fuelStationPlan.NearDistance);
            }
        }

        public async Task PublishEvent(Guid truckId, Guid fuelStationId, string address, double longitude, double latitude, double distanceKm)
        {
            await publishEndpoint.Publish(new FuelStationPlanMarkedAsNearIntegretionEvent(truckId, fuelStationId, address, longitude, latitude, distanceKm));
        }
    }

    public static class GeoUtils
    {
        private const double EarthRadiusKm = 6371.0;

        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}

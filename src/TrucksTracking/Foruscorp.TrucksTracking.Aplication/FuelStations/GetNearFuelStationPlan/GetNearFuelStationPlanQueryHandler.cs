using FluentResults;
using Foruscorp.BuildingBlocks.Domain.GeoUtilsTools;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.FuelStations.GetNearFuelStationPlan
{
    public record GetNearFuelStationPlanBonusIndexQuery(Guid TruckId, DateTime TrunsactionTime, double Quantity, double TankCapacity = 200) : IRequest<int>;

    public class GetNearFuelStationPlanQueryHandler(
        IFuelStationService fuelStationService,
        ITruckTrackingContext truckTrackingContext,
        ILogger<GetNearFuelStationPlanQueryHandler> logger) : IRequestHandler<GetNearFuelStationPlanBonusIndexQuery, int>
    {
        private readonly ILogger<GetNearFuelStationPlanQueryHandler> _logger = logger;

        public async Task<int> Handle(GetNearFuelStationPlanBonusIndexQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting GetNearFuelStationPlanBonusIndex for TruckId {TruckId} at {TrunsactionTime}", request.TruckId, request.TrunsactionTime);

            var planList = await truckTrackingContext.NearFuelStationPlans
                .Where(p => p.TruckId == request.TruckId && !p.IsProcessed)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {PlanCount} unprocessed fuel plans for TruckId {TruckId}", planList.Count, request.TruckId);

            int bonusIndex = 0;

            foreach (var plan in planList)
            {
                var fromTime = request.TrunsactionTime.AddMinutes(-15);
                var toTime = request.TrunsactionTime.AddMinutes(15);

                var truckLocation = await truckTrackingContext.TruckLocations
                    .Where(t => t.TruckId == request.TruckId
                                && t.RecordedAt >= fromTime
                                && t.RecordedAt <= toTime)
                    .OrderByDescending(t => t.RecordedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (truckLocation == null)
                {
                    _logger.LogWarning("No truck location found for TruckId {TruckId} within ±15 minutes of {TrunsactionTime}", request.TruckId, request.TrunsactionTime);
                    continue;
                }

                var distance = GeoUtils.CalculateDistance(
                    truckLocation.Location.Latitude, truckLocation.Location.Longitude,
                    plan.Latitude, plan.Longitude);

                _logger.LogInformation("Distance from truck to plan fuel station {FuelStationId} is {Distance} km", plan.FuelStationId, distance);

                if (distance >= 0.5)
                {
                    _logger.LogInformation("Truck is not near the fuel station {FuelStationId}", plan.FuelStationId);
                    continue;
                }

                var quantityPercenteg = (request.Quantity / request.TankCapacity) * 100;
                var planRefillPercenteg = (plan.Refill / request.TankCapacity) * 100;

                if (Math.Abs(quantityPercenteg - planRefillPercenteg) <= 5)
                {
                    bonusIndex = 2;
                    _logger.LogInformation("Correct refill amount for FuelStationId {FuelStationId}. Awarding 100 points", plan.FuelStationId);
                }
                else
                {
                    bonusIndex = 1;
                    _logger.LogInformation("Incorrect refill amount for FuelStationId {FuelStationId}. Awarding 50 points", plan.FuelStationId);
                }

                plan.MarkAsProcessed();
                truckTrackingContext.NearFuelStationPlans.Update(plan);

                break; 
            }

            await truckTrackingContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Finished GetNearFuelStationPlanBonusIndex for TruckId {TruckId}. BonusIndex: {BonusIndex}", request.TruckId, bonusIndex);

            return bonusIndex;
        }
    }
}
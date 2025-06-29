using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged
{
    public class UpdateTruckTrackerIfChangedCommandHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<UpdateTruckTrackerCommandHandler> logger,
        TruckInfoManager truckInfoManager) : IRequestHandler<UpdateTruckTrackerIfChangedCommand>
    {
        public async Task Handle(UpdateTruckTrackerIfChangedCommand request, CancellationToken cancellationToken)
        {
            if (!request.TruckStatsUpdates.Any())
            {
                logger.LogWarning("No truck stats updates provided.");
                return;
            }

            foreach (var truckStatsUpdate in request.TruckStatsUpdates)
            {
                var isFuelChanged = truckInfoManager.UpdateTruckIFuelnfoIfChanged(truckStatsUpdate);
                if (isFuelChanged)
                    logger.LogInformation("TruckId: {TruckId}, Fuel changed", truckStatsUpdate.TruckId);

                var isLocationChanged = truckInfoManager.UpdateTruckLocationInfoIfChanged(truckStatsUpdate);
                if (isLocationChanged)
                    logger.LogInformation("TruckId: {TruckId}, Location changed", truckStatsUpdate.TruckId);

                if (isLocationChanged || isFuelChanged)
                    await UpdateTrukTracker(truckStatsUpdate);

                //logger.LogInformation("TruckId: {TruckId}, FuelChanged: {IsFuelChanged}, LocationChanged: {IsLocationChanged}",
                //    truckStatsUpdate.TruckId, isFuelChanged, isLocationChanged);

                //logger.LogInformation("Processing TruckId: {TruckId}, IsChanged: {IsChanged}",
                //    TruckStatsUpdate.TruckId, isFuelChanged);
            }
        }

        public async Task UpdateTrukTracker(TruckInfoUpdate truckStatsUpdate)
        {
            var truckId = Guid.Parse(truckStatsUpdate.TruckId);

            var truckTracker = await tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.TruckLocationHistory)
                .FirstOrDefaultAsync(t => t.TruckId == truckId);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", truckId);
                return;
            }

            truckTracker.UpdateTruck(
                new GeoPoint(truckStatsUpdate.Latitude, truckStatsUpdate.Longitude),
                truckStatsUpdate.formattedLocation,
                truckStatsUpdate.fuelPercents);

            logger.LogInformation("Updated Truck Tracker for TruckId: {TruckId}", truckId);

            await tuckTrackingContext.SaveChangesAsync();
        }
    }
}

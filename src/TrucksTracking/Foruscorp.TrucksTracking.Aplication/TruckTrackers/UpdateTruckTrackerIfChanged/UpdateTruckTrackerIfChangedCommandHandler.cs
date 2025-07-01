using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckStatus;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged
{
    public class UpdateTruckTrackerIfChangedCommandHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<UpdateTruckTrackerCommandHandler> logger,
        TruckInfoManager truckInfoManager,
        ISender sender) : IRequestHandler<UpdateTruckTrackerIfChangedCommand>
    {
        public async Task Handle(UpdateTruckTrackerIfChangedCommand request, CancellationToken cancellationToken)
        {
            if (!request.TruckStatsUpdates.Any())
            {
                logger.LogWarning("No truck stats updates provided.");
                return;
            }

            foreach (var truckStatsUpdate in request.TruckStatsUpdates.DistinctBy(t => t.TruckId))
            {

                var isLocationChanged = truckInfoManager.UpdateTruckLocationInfoIfChanged(truckStatsUpdate);
                if (isLocationChanged)
                    logger.LogInformation("TruckId: {TruckId}, Location changed", truckStatsUpdate.TruckId);


                var isFuelChanged = truckInfoManager.UpdateTruckIFuelnfoIfChanged(truckStatsUpdate);
                if (isFuelChanged)
                    logger.LogInformation("TruckId: {TruckId}, Fuel changed", truckStatsUpdate.TruckId);

                var isEngineSatusChanged = truckInfoManager.UpdateTruckEngineInfoIfChanged(truckStatsUpdate);
                if (isEngineSatusChanged)
                {
                    await sender.Send(new UpdateTruckStatusCommand() {TruckId = Guid.Parse(truckStatsUpdate.TruckId), EngineStatus = truckStatsUpdate.engineStateData.Value }, cancellationToken);  
                    logger.LogInformation("TruckId: {TruckId}, Engine status changed", truckStatsUpdate.TruckId);
                }


                if (isLocationChanged || isFuelChanged)
                    await UpdateTrukTracker(truckStatsUpdate, isLocationChanged, isFuelChanged);
            }
        }

        public async Task UpdateTrukTracker(TruckInfoUpdate truckStatsUpdate, bool isLocationChanged = false, bool IsFuelOnly = false)
        {
            var truckId = Guid.Parse(truckStatsUpdate.TruckId);

            var truckTracker = await tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentRoute)
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.TruckLocationHistory)
                .FirstOrDefaultAsync(t => t.TruckId == truckId);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", truckId);
                return;
            }

            if (IsFuelOnly)
            {
                truckTracker.UpdateFuelStatus(truckStatsUpdate.fuelPercents);
            }
            else if(isLocationChanged)
            {
                truckTracker.UpdateCurrentTruckLocation(
                    new GeoPoint(truckStatsUpdate.Latitude, truckStatsUpdate.Longitude),
                    truckStatsUpdate.formattedLocation);
            }

            logger.LogInformation("Updated Truck Tracker for TruckId: {TruckId}", truckId);

            await tuckTrackingContext.SaveChangesAsync();
        }
    }
}

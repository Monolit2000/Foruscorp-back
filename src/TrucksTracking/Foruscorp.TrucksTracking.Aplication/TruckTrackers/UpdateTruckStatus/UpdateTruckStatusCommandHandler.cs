using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckStatus
{
    public class UpdateTruckStatusCommandHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<UpdateTruckTrackerCommandHandler> logger,
        ISignalRNotificationSender signalRNotificationSender,
        IPublishEndpoint publishEndpoint) : IRequestHandler<UpdateTruckStatusCommand>
    {
        public async Task Handle(UpdateTruckStatusCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentRoute)
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.TruckLocationHistory)
                .FirstOrDefaultAsync(t => t.TruckId == request.TruckId);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", request.TruckId);
                return;
            }

            TruckEngineStatus status = request.EngineStatus.ToLower() switch
            {
                "on" => TruckEngineStatus.On,
                "off" => TruckEngineStatus.Off,
                _ => TruckEngineStatus.Idle
            };

            truckTracker.UpdateTruckEnginStatus(status);

            await tuckTrackingContext.SaveChangesAsync();

            await signalRNotificationSender.SendTruckStatusUpdateAsync(
               new TruckStausUpdate(truckTracker.TruckId.ToString(), (int)status));

            await publishEndpoint.Publish(new TruckStatusChengedIntegreationEvent
            {
                TruckId = truckTracker.TruckId,
                Status = (int)status,
            });
        }
    }
}

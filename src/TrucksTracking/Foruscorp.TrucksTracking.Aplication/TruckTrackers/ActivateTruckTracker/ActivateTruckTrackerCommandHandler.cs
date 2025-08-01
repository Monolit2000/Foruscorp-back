﻿using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using static System.Net.Mime.MediaTypeNames;


namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.ActivateTruckTracker
{
    public class ActivateTruckTrackerCommandHandler(
        ActiveTruckManager activeTruckManager,
        ITruckTrackingContext tuckTrackingContext) : IRequestHandler<ActivateTruckTrackerCommand, Result<TruckTrackerDto>>
    {
        public async Task<Result<TruckTrackerDto>> Handle(ActivateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId);
            
            if (truckTracker == null)
                return Result.Fail("Truck Tracker not exist");
            
            truckTracker.ActivateTruckTrucker();

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);

            activeTruckManager.AddTruck(truckTracker.TruckId.ToString());   

            return Result.Ok(new TruckTrackerDto
            {
                TrackerId = truckTracker.Id,
                TruckId = truckTracker.TruckId,
                Status = truckTracker.TruckStatus.ToString()
            }); 
        }
    }
}

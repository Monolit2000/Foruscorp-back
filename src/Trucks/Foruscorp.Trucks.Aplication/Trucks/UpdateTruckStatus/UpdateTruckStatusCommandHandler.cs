using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Trucks;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.UpdateTruckStatus
{
    public class UpdateTruckStatusCommandHandler(
        ITruckContext truckContext,
        ILogger<UpdateTruckStatusCommandHandler> logger) : IRequestHandler<UpdateTruckStatusCommand>
    {
        public async Task Handle(UpdateTruckStatusCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating truck status for TruckId: {TruckId} to Status: {Status}", request.TruckId, request.Status);

            var truck = await truckContext.Trucks
                .FirstOrDefaultAsync(t => t.Id == request.TruckId);

            if (truck == null)
                logger.LogWarning("Truck not found with ID: {TruckId}", request.TruckId);

            truck.UpdateStatus((TruckStatus)request.Status);

            await truckContext.SaveChangesAsync(cancellationToken); 
        }
    }
}

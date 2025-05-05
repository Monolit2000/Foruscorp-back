using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Trucks;
using MediatR;

using Foruscorp.Trucks.Aplication.Trucks;
using MassTransit;
using Foruscorp.Trucks.IntegrationEvents;

namespace Foruscorp.Trucks.Aplication.Trucks.CreateTruck
{
    public class CreateTruckCommandHandler(
        ITruckContext context,
        IPublishEndpoint publishEndpoint) : IRequestHandler<CreateTruckCommand, TruckDto>
    {
        public async Task<TruckDto> Handle(CreateTruckCommand request, CancellationToken cancellationToken)
        {
            //var truck = Truck.CreateNew(
            //    request.Ulid,
            //    request.LicensePlate);

            //await context.Trucks.AddAsync(truck, cancellationToken);

            //await context.SaveChangesAsync(cancellationToken);

            //var truckDto = truck.ToTruckDto();

            //await publishEndpoint.Publish(new TruckCreatedIntegrationEvent() { TruckId = truckDto.Id });

            //return truckDto;    

            throw new NotImplementedException("Truck creation is not implemented yet.");    
        }
    }
}

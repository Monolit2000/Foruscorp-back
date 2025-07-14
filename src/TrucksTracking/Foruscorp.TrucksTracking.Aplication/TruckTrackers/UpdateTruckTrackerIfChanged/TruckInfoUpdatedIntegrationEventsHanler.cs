using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Worker.IntegrationEvents;
using MassTransit;
using MassTransit.NewIdProviders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged
{
    public class TruckInfoUpdatedIntegrationEventsHanler(
        ISender sender) : IConsumer<TruckInfoUpdatedIntegrationEvents>
    {
        public async Task Consume(ConsumeContext<TruckInfoUpdatedIntegrationEvents> context)
        {

            var truckInfos = context.Message.truckInfoUpdatedIntegrationEvents
                .Select(truckInfo => new TruckInfoUpdate(
                    truckInfo.TruckId,
                    truckInfo.TruckName,
                    truckInfo.Longitude,
                    truckInfo.Latitude,
                    truckInfo.Time,
                    truckInfo.HeadingDegrees,
                    truckInfo.fuelPercents,
                    truckInfo.formattedLocation,
                    new Contruct.TruckProvider.EngineStateData 
                    {
                        Time = truckInfo.engineStateTime , 
                        Value = truckInfo.engineStateValue 
                    }
                )).ToList();

            var command = new UpdateTruckTrackerIfChangedCommand { TruckStatsUpdates  = truckInfos };
            await sender.Send(command);
        }
    }
}

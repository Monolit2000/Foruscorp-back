using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.ActivateTruckTracker;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.DeactivateTruckTracker;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetAllTruckTrackers;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.SetCurrentRoute;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Foruscorp.TrucksTracking.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrucksTrackingController(
        IMediator mediator,
        ITruckProviderService truckProviderService) : ControllerBase
    {
        [HttpGet("get-all-truck-trackers")]
        public async Task<ActionResult> GetFuelStationsByRadiusGet(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllTruckTrackersQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("activate-truck-tracker")]
        public async Task<ActionResult> ActivateTruckTracker(ActivateTruckTrackerCommand activateTruckTrackerCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(activateTruckTrackerCommand, cancellationToken);
            return Ok(result);
        }


        [HttpPost("deactivate-truck-tracker")]
        public async Task<ActionResult> DeactivateTruckTracker(DeactivateTruckTrackerCommand deactivateTruckTrackerCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(deactivateTruckTrackerCommand, cancellationToken);
            return Ok(result);
        }


        [HttpPost("get-vehicle-stats")]
        public async Task<ActionResult> Deactivate(CancellationToken cancellationToken)
        {
            var result = await truckProviderService.GetHistoricalStatsAsync(new List<string>(), DateTime.UtcNow, cancellationToken); 
            return Ok(result);
        }

        [HttpPost("GetVehicleStatsFeedAsync")]
        public async Task<ActionResult> GetVehicleStatsFeedAsync(CancellationToken cancellationToken)
        {
            var result = await truckProviderService.GetVehicleStatsFeedAsync(new List<string>(), DateTime.UtcNow, cancellationToken); 
            return Ok(result);
        }

        [HttpPost("UpdateTruckTrackerCommand")]
        public async Task<ActionResult> GetVehicleStatsFeedAsync(UpdateTruckTrackerCommand updateTruckTrackerCommand)
        {
            await mediator.Send(updateTruckTrackerCommand);
            return Ok();
        }


        [HttpPost("SetCurrentRouteCommand")]
        public async Task<ActionResult> SetCurrentRouteCommand(SetCurrentRouteCommand setCurrentRouteCommand)
        {
            await mediator.Send(setCurrentRouteCommand);
            return Ok();
        }


    }
}

using Foruscorp.TrucksTracking.Aplication.TruckTrackers.ActivateTruckTracker;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetAllTruckTrackers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.TrucksTracking.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrucksTrackingController(IMediator mediator) : ControllerBase
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
    }
}

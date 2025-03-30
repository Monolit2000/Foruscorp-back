using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Foruscorp.FuelStations.Aplication.FuelStations;

namespace Foruscorp.FuelStations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuelStationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FuelStationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("by-radius")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> GetFuelStationsByRadius(
            [FromQuery] int radius,
            CancellationToken cancellationToken)
        {
            var query = new GetFuelStationsByRadiusQuery { Radius = radius };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}

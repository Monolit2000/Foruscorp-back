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

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FuelStationDto>))]
        [HttpPost("by-radius")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> GetFuelStationsByRadius(
            GetFuelStationsByRadiusQuery getFuelStationsByRadiusQuery,
            CancellationToken cancellationToken)
        {
            var query = getFuelStationsByRadiusQuery;
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }


        [HttpGet("get-by-radius")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> GetFuelStationsByRadiusGet([FromQuery]
          GetFuelStationsByRadiusQuery getFuelStationsByRadiusQuery,
          CancellationToken cancellationToken)
        {
            var query = getFuelStationsByRadiusQuery;
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}

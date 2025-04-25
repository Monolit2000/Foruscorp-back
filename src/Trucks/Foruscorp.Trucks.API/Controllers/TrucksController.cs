using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foruscorp.Trucks.Aplication.Trucks.CreateTruck;
using Foruscorp.Trucks.Aplication.Trucks.GetAllTruks;
using Foruscorp.Trucks.Aplication.Drivers.CreateDriver;
using Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers;
using Foruscorp.Trucks.Aplication.Trucks.AttachDriver;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Foruscorp.Trucks.Aplication.Trucks;
using FluentResults;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrucksController(
        IMediator mediator) : ControllerBase
    {

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TruckDto>))]
        [HttpPost("create-truck")]
        public async Task<ActionResult> GetFuelStationsByRadius( CreateTruckCommand createTruckCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createTruckCommand, cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Result>))]
        [HttpPost("attach-driver")]
        public async Task<ActionResult> AttachDriver(AttachDriverCommand attachDriverCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(attachDriverCommand, cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TruckDto>))]
        [HttpGet("get-truck-list")]
        public async Task<ActionResult> GetFuelStationsByRadiusGet( CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllTruksQuery(), cancellationToken);
            return Ok(result);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DriverDto))]
        [HttpPost("create-driver")]
        public async Task<ActionResult> CreateDriver(CreateDriverCommand createDriverCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createDriverCommand, cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DriverDto>))]
        [HttpGet("get-all-drivers")]
        public async Task<ActionResult> GetAllDrivers(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllDriversQuery(), cancellationToken);
            return Ok(result);
        }

    }
}

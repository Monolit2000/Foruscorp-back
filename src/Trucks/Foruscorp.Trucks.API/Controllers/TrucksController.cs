
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foruscorp.Trucks.Aplication.Trucks.CreateTruck;
using Foruscorp.Trucks.Aplication.Trucks.GetAllTruks;
using Foruscorp.Trucks.Aplication.Drivers.CreateDriver;
using Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers;
using Foruscorp.Trucks.Aplication.Trucks.AttachDriver;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrucksController(
        IMediator mediator) : ControllerBase
    {

        [HttpPost("create-truck")]
        public async Task<ActionResult> GetFuelStationsByRadius( CreateTruckCommand createTruckCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createTruckCommand, cancellationToken);
            return Ok(result);
        }


        [HttpPost("attach-driver")]
        public async Task<ActionResult> AttachDriver(AttachDriverCommand attachDriverCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(attachDriverCommand, cancellationToken);
            return Ok(result);
        }


        [HttpGet("get-truck-list")]
        public async Task<ActionResult> GetFuelStationsByRadiusGet( CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllTruksQuery(), cancellationToken);
            return Ok(result);
        }


        [HttpPost("create-driver")]
        public async Task<ActionResult> CreateDriver(CreateDriverCommand createDriverCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createDriverCommand, cancellationToken);
            return Ok(result);
        }


        [HttpGet("get-all-drivers")]
        public async Task<ActionResult> GetAllDrivers(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllDriversQuery(), cancellationToken);
            return Ok(result);
        }

    }
}

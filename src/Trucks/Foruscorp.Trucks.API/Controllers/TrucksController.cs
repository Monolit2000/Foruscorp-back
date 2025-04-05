using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Foruscorp.Trucks.Aplication.Trucks.CreateTruck;
using Foruscorp.Trucks.Aplication.Trucks.GetAllTruks;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrucksController(
        IMediator mediator) : ControllerBase
    {

        [HttpPost("createTruck")]
        public async Task<ActionResult> GetFuelStationsByRadius(
            CreateTruckCommand createTruckCommand,
            CancellationToken cancellationToken)
        {
            var command = createTruckCommand;
            var result = await mediator.Send(command, cancellationToken);
            return Ok(result);
        }


        [HttpGet("get-truck-list")]
        public async Task<ActionResult> GetFuelStationsByRadiusGet(
          GetAllTruksQuery getFuelStationsByRadiusQuery,
          CancellationToken cancellationToken)
        {
            var query = getFuelStationsByRadiusQuery;
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

    }
}

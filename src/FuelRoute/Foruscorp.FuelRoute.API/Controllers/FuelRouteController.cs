using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.DropPiont;
using FluentResults;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Foruscorp.FuelRoutes.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuelRouteController(
        ISender mediator,
        ITruckerPathApi truckerPathApi) : ControllerBase
    {


        [HttpPost("canselation-create-fuel-route-canselation")]
        public async Task<ActionResult> GetFuelStationsByRadiusCancellation([FromBody] int testvalue, CancellationToken cancellationToken)
        {
            Console.WriteLine("Start____________________Start");
            await Task.Delay(5000, cancellationToken);
            Console.WriteLine("End____________________End");
            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FuelRouteDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("create-fuel-route")]
        public async Task<ActionResult> GetFuelStationsByRadius(CreateFuelRouteCommand createFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createFuelRouteCommand, cancellationToken);
            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }


        [HttpPost("accept-fuel-route")]
        public async Task<ActionResult> AcceptFuelRouteCommand(AcceptFuelRouteCommand acceptFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(acceptFuelRouteCommand, cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SimpleDropPointResponse))]
        [HttpPost("drop-point")]
        public async Task<ActionResult> DropPoint([FromBody] GeoPoint request, CancellationToken cancellationToken)
        {
            var result = await truckerPathApi.DropPoint(request.Latitude, request.Longitude, cancellationToken: cancellationToken); 
            return Ok(result);  
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SimpleDropPointResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("drop-point-V2")]
        public async Task<ActionResult> DropPointV2([FromBody] GeoPoint request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DropPiontCommand() { latitude = request.Latitude, longitude = request.Longitude }); 

            if(result.IsSuccess)
                return Ok(result.Value);  

            return NotFound(result.Errors); 
        }
    }
}

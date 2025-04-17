using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using System;

namespace Foruscorp.FuelRoutes.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuelRouteController(
        ISender mediator,
        ITruckerPathApi truckerPathApi) : ControllerBase
    {


        [HttpPost("canselation-create-fuel-route-canselation")]
        public async Task<ActionResult> GetFuelStationsByRadiusCancellation(CreateFuelRouteCommand createFuelRouteCommand, CancellationToken cancellationToken)
        {
            Console.WriteLine("Start____________________Start");

            await Task.Delay(5000, cancellationToken);

            Console.WriteLine("End____________________End");

            return Ok();
        }


        [HttpPost("create-fuel-route")]
        public async Task<ActionResult> GetFuelStationsByRadius(CreateFuelRouteCommand createFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createFuelRouteCommand, cancellationToken);
            return Ok(result);
        }


        [HttpPost("accept-fuel-route")]
        public async Task<ActionResult> AcceptFuelRouteCommand(AcceptFuelRouteCommand acceptFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(acceptFuelRouteCommand, cancellationToken);
            return Ok(result);
        }


        [HttpPost("drop-point")]
        public async Task<ActionResult> DropPoint([FromBody] GeoPoint request, CancellationToken cancellationToken)
        {
            var result = await truckerPathApi.DropPoint(request.Latitude, request.Longitude, cancellationToken: cancellationToken); 
            return Ok(result);  
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using MassTransit.Mediator;
using Foruscorp.FuelRoutes.Aplication.FuelRoute.CreateFuelRoute;
using MediatR;

namespace Foruscorp.FuelRoutes.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuelRouteController(
        ISender mediator) : ControllerBase
    {
  
  


        [HttpPost("create-truck")]
        public async Task<ActionResult> GetFuelStationsByRadius(CreateFuelRouteCommand createFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createFuelRouteCommand, cancellationToken);
            return Ok(result);
        }


    }
}

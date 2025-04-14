using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute;

namespace Foruscorp.FuelRoutes.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuelRouteController(
        ISender mediator) : ControllerBase
    {
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
    }
}

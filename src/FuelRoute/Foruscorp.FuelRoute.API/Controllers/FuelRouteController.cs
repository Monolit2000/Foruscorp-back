using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AddFuelStation;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AssignRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.DropPiont;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.GenerateFuelStations;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuelRouteController(
        ISender mediator,
        ITruckerPathApi truckerPathApi) : ControllerBase
    {

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

        [HttpPost("get-fuel-route")]
        public async Task<ActionResult> GetFuelRouteQuery(GetPussedFuelRouteQuery getFuelRouteQuery, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(getFuelRouteQuery);

            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FuelStationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("get-fuel-stations")]
        public async Task<IActionResult> GetFuelStation(GetFuelStationsCommand getFuelStationsCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(getFuelStationsCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FuelStationDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("add-fuel-station")]
        public async Task<IActionResult> AddFuelStation(AddFuelStationCommand addFuelStationCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(addFuelStationCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [HttpPost("get-current-route")]
        public async Task<IActionResult> GetFuelRoute(GetPussedFuelRouteQuery getFuelRouteQuery, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(getFuelRouteQuery);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [HttpPost("AssignRoute")]
        public async Task<IActionResult> AssignRoute(AssignRouteCommand assignRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(assignRouteCommand);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Errors);
        }


    }
}

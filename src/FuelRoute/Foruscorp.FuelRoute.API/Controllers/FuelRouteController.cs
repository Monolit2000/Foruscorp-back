using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AddFuelStation;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.AssignRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.ChangeFuelPlan;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.DropPiont;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.EditFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetAasignedRouteByTruckId;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.PlanFuelStations;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.SelfAssignRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.CompleteRoute;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.DeclineFuelRoute;
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

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoutInfoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
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
        public async Task<IActionResult> GetFuelStation(PlanFuelStationsCommand getFuelStationsCommand, CancellationToken cancellationToken)
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

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoutInfoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("get-current-route")]
        public async Task<IActionResult> GetFuelRoute(GetPussedFuelRouteQuery getFuelRouteQuery, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(getFuelRouteQuery);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("AssignRoute")]
        public async Task<IActionResult> AssignRoute(AssignRouteCommand assignRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(assignRouteCommand);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Errors);
        }

        [HttpPost("selfAssignRoute")]
        public async Task<IActionResult> SelfAssignRouteCommand(SelfAssignRouteCommand selfAssignRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(selfAssignRouteCommand);

            if (result.IsSuccess)
                return Ok();

            return BadRequest(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetFuelRouteDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("get-fuel-route-byId")]
        public async Task<IActionResult> GetByUser(GetFuelRouteQuery getFuelRouteQuery )
        {
            var list = await mediator.Send(getFuelRouteQuery);
            return Ok(list);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAasignedRouteByTruckIdResponce))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("get-assigned-route-by-truck-Id")]
        public async Task<IActionResult> GetAasignedRouteByTruckIdQuery(GetAasignedRouteByTruckIdQuery getAasignedRouteByTruckIdQuery )
        {
            var result = await mediator.Send(getAasignedRouteByTruckIdQuery);
            return Ok(result.Value);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FuelRouteDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("edit-fuel-route")]
        public async Task<IActionResult> EditFuelRouteCommand(EditFuelRouteCommand editFuelRouteCommand )
        {
            var result = await mediator.Send(editFuelRouteCommand);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("complete-route")]
        public async Task<IActionResult> CompleteRoute(CompleteRouteCommand completeRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(completeRouteCommand, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Successes);

            return BadRequest(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("decline-fuel-route")]
        public async Task<IActionResult> DeclineFuelRoute(DeclineFuelRouteCommand declineFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(declineFuelRouteCommand, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Successes);

            return BadRequest(result.Errors);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangeFuelPlanResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("change-fuel-plan")]
        public async Task<IActionResult> ChangeFuelPlan(ChangeFuelPlanCommand changeFuelPlanCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(changeFuelPlanCommand, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Errors);
        }
        public async Task<IActionResult> DeclineFuelRoute(DeclineFuelRouteCommand declineFuelRouteCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(declineFuelRouteCommand, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Successes);

            return BadRequest(result.Errors);
        }


    }
}

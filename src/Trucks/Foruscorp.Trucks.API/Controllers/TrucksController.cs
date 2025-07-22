using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Contruct.Samasara;
using Foruscorp.Trucks.Aplication.DriverBonuses.DecreaseBonus;
using Foruscorp.Trucks.Aplication.DriverBonuses.IncreaseBonus;
using Foruscorp.Trucks.Aplication.Drivers;
using Foruscorp.Trucks.Aplication.Drivers.CreateDriver;
using Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers;
using Foruscorp.Trucks.Aplication.Drivers.SetUser;
using Foruscorp.Trucks.Aplication.Drivers.UpdateDriverContact;
using Foruscorp.Trucks.Aplication.Trucks;
using Foruscorp.Trucks.Aplication.Trucks.AttachDriver;
using Foruscorp.Trucks.Aplication.Trucks.CreateTruck;
using Foruscorp.Trucks.Aplication.Trucks.GetAllTruks;
using Foruscorp.Trucks.Aplication.Trucks.GetTruckById;
using Foruscorp.Trucks.Aplication.Trucks.GetTruckByUserId;
using Foruscorp.Trucks.Aplication.Trucks.LoadTrucks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrucksController(
        IMediator mediator,
        ITruckProviderService truckProviderService) : ControllerBase
    {
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TruckDto>))]
        [HttpPost("create-truck")]
        public async Task<ActionResult> GetFuelStationsByRadius( CreateTruckCommand createTruckCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createTruckCommand, cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TruckDto))]
        [HttpGet("get-truckBy-id")]
        public async Task<ActionResult> GetTruckById(Guid truckId, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetTruckByIdQuery { TruckId = truckId }, cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.Errors);   

            return Ok(result.Value);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FluentResults.Result>))]
        [HttpPost("attach-driver")]
        public async Task<ActionResult> AttachDriver(AttachDriverCommand attachDriverCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(attachDriverCommand, cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TruckDto>))]
        [HttpGet("get-truck-list")]
        public async Task<ActionResult> GetTruckList( CancellationToken cancellationToken)
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


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DriverDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("update-driver-contact")]
        public async Task<ActionResult> CreateDriver(UpdateDriverContactCommand updateDriverContactCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(updateDriverContactCommand, cancellationToken);
            return Ok(result);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("increase-driver-bonus")]
        public async Task<ActionResult> IncreaseBonus(IncreaseBonusCommand increaseBonusCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(increaseBonusCommand, cancellationToken);
            return Ok(result);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("decrease-driver-bonus")]
        public async Task<ActionResult> DecreaseBonus(DecreaseBonusCommand decreaseBonusCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(decreaseBonusCommand, cancellationToken);
            return Ok(result);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DriverDto>))]
        [HttpGet("get-all-drivers")]
        public async Task<ActionResult> GetAllDrivers(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllDriversQuery(), cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VehicleResponse))]
        [HttpGet("get-fleat")]
        public async Task<ActionResult> GetTruckFormSamsara(CancellationToken cancellationToken)
        {
            var result = await truckProviderService.GetVehiclesAsync(cancellationToken);    
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VehicleStatsResponse))]
        [HttpGet("fleet-locations")]
        public async Task<ActionResult> GetFleetLocations([FromQuery] string after = null, CancellationToken cancellationToken = default)
        {
            var result = await truckProviderService.GetVehicleLocationsFeedAsync(after);
            return Ok(result);
        }

     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VehicleStatsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("vehicle-stats")]
        public async Task<ActionResult> GetVehicleStats(string vehicleId = null,string after = null, CancellationToken cancellationToken = default)
        {
            var result = await truckProviderService.GetVehicleStatsFeedAsync(vehicleId, after);

            result.Data = result.Data
                        .Where(vs => vs.EngineStates?.Any(es => es.Value == "On") == true)
                        .ToArray();

            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TruckDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("load-trucks")]
        public async Task<ActionResult> LoadTrucks(CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new LoadTrucksCommand(), cancellationToken);
            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }


        [HttpPost("set-user")]
        public async Task<ActionResult> SetUserCommand(SetUserCommand setUserCommand, CancellationToken cancellationToken = default)
        {
            await mediator.Send(setUserCommand, cancellationToken);

            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TruckDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<TruckDto>> GetByUserId(
            [FromRoute] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetTruckByUserIdQuery(userId), cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }



    }

    public class Request()
    {
        public string VehicleId { get; set; }
        public string After { get; set; }   
    }

}

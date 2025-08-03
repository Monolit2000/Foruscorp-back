using System;
using MediatR;
using FluentResults;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Foruscorp.Trucks.Aplication.Trucks;
using Foruscorp.Trucks.Aplication.Drivers;
using Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers;
using Foruscorp.Trucks.Aplication.Drivers.GetDriverById;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriverController(IMediator mediator) : ControllerBase
    {
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DriverDto>))]
        [HttpGet("get-all")]
        public async Task<ActionResult> GetAllDrivers(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllDriversQuery(), cancellationToken);
            return Ok(result);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DriverDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpGet("driver/{id:guid}")]
        public async Task<ActionResult<TruckDto>> GetDriverById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetDriverByIdQuery(id), cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }


    }
}

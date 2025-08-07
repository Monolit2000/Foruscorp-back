using FluentResults;
using Foruscorp.Trucks.Aplication.Companys;
using Foruscorp.Trucks.Aplication.Companys.CreateCompany;
using Foruscorp.Trucks.Aplication.Companys.GetCompanyById;
using Foruscorp.Trucks.Aplication.Companys.GetCompanys;
using Foruscorp.Trucks.Aplication.DriverBonuses.IncreaseBonus;
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
    public class CompanyController(IMediator mediator) : ControllerBase
    {
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ISuccess))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpPost("add")]
        public async Task<ActionResult> IncreaseBonus(CreateCompanyCommand createCompanyCommand, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(createCompanyCommand, cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Successes.First().Message);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpGet("all")]
        public async Task<ActionResult> GetCompanys(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetCompanysQuery(), cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<IError>))]
        [HttpGet("by-id/{id:guid}")]
        public async Task<ActionResult> GetCompanyByIdQuery([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }
    }
}

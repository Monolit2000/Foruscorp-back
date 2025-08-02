using FluentResults;
using Foruscorp.Trucks.Aplication.Companys;
using Foruscorp.Trucks.Aplication.DriverBonuses.IncreaseBonus;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }
}

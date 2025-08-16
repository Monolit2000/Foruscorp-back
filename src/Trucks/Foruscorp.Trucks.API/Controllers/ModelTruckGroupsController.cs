using Foruscorp.Trucks.Aplication.ModelTruckGroups.FormModelTruckGroupsForAllTrucks;
using Foruscorp.Trucks.Aplication.ModelTruckGroups.GetAllModelTruckGroups;
using Foruscorp.Trucks.Aplication.ModelTruckGroups.SetTruckGroupFuelCapacity;
using Foruscorp.Trucks.Aplication.ModelTruckGroups.SetTruckGroupWeight;
using Foruscorp.Trucks.Aplication.ModelTruckGroups.SetTruckGroupWeightFuelCapacity;
using Foruscorp.Trucks.Aplication.Trucks.Commands;
using Foruscorp.Trucks.Aplication.Trucks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModelTruckGroupsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ModelTruckGroupsController> _logger;

        public ModelTruckGroupsController(
            IMediator mediator,
            ILogger<ModelTruckGroupsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        //[HttpGet]
        //public async Task<ActionResult<GetModelTruckGroupsResult>> GetModelTruckGroups(
        //    [FromQuery] int page = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string? searchTerm = null)
        //{
        //    try
        //    {
        //        var query = new GetModelTruckGroupsQuery
        //        {
        //            Page = page,
        //            PageSize = pageSize,
        //            SearchTerm = searchTerm
        //        };

        //        var result = await _mediator.Send(query);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while getting ModelTruckGroups");
        //        return StatusCode(500, "An error occurred while retrieving ModelTruckGroups");
        //    }
        //}


        [HttpGet("All")]
        public async Task<ActionResult<FormModelTruckGroupForAllTruksCommand>> GetAllModelTruckGroupsQuery()
        {
            var result = await _mediator.Send(new GetAllModelTruckGroupsQuery());

            return Ok(result);
        }


        [HttpPost("form-for-all-trucks")]
        public async Task<ActionResult<FormModelTruckGroupForAllTruksCommand>> FormModelTruckGroupsForAllTrucks()
        {
            try
            {
                var command = new FormModelTruckGroupForAllTruksCommand();
            

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while forming ModelTruckGroups for all trucks");
                return StatusCode(500, "An error occurred while forming ModelTruckGroups");
            }
        }


        [HttpPost("set-fuelCapacity")]
        public async Task<ActionResult<FormModelTruckGroupForAllTruksCommand>> SetTruckGroupFuelCapacityCommand(SetTruckGroupFuelCapacityCommand setTruckGroupFuelCapacityCommand  )
        {
            var result = await _mediator.Send(setTruckGroupFuelCapacityCommand);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result);

        }

        [HttpPost("set-Weight")]
        public async Task<ActionResult<FormModelTruckGroupForAllTruksCommand>> SetTruckGroupWeightCommand(SetTruckGroupWeightCommand setTruckGroupWeightCommand)
        {
            var result = await _mediator.Send(setTruckGroupWeightCommand);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result);

        }

        [HttpPost("set-WeightAndFuelCapacit")]
        public async Task<ActionResult<FormModelTruckGroupForAllTruksCommand>> SetTruckGroupWeightFuelCapacityCommand(SetTruckGroupWeightFuelCapacityCommand setTruckGroupWeightFuelCapacityCommand)
        {
            var result = await _mediator.Send(setTruckGroupWeightFuelCapacityCommand);

            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result);

        }

    }
}

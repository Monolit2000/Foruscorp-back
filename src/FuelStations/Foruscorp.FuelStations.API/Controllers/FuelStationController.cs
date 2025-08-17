using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Aplication.FuelStations;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadInfo;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadPrice;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadLovesStores;
using Foruscorp.FuelStations.Aplication.FuelStations.LodadFuelStation;
using Foruscorp.FuelStations.Infrastructure.WebScrapers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.FuelStations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuelStationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IXMlFuelStationService _xMlFuelStationService;
        private readonly IFuelStationsService _fuelStationsService;
        public FuelStationController(
            IMediator mediator, 
            IXMlFuelStationService xMlFuelStationService,
            IFuelStationsService fuelStationsService)
        {
            _mediator = mediator;
            _xMlFuelStationService = xMlFuelStationService;
            _fuelStationsService = fuelStationsService; 
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FuelStationDto>))]
        [HttpPost("by-radius")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> GetFuelStationsByRadius(
            GetFuelStationsByRadiusQuery getFuelStationsByRadiusQuery,
            CancellationToken cancellationToken)
        {
            var query = getFuelStationsByRadiusQuery;
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("lodad-fuelStations")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> LodadFuelStations(
            CancellationToken cancellationToken)
        {
            var query = new LodadFuelStationCommand();
            await _mediator.Send(query, cancellationToken);
            return Ok();
        }



        [HttpGet("get-by-radius")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> GetFuelStationsByRadiusGet([FromQuery]
          GetFuelStationsByRadiusQuery getFuelStationsByRadiusQuery,
          CancellationToken cancellationToken)
        {
            var query = getFuelStationsByRadiusQuery;
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }


        [HttpPost("parce")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> Parce(IFormFile file,
          CancellationToken cancellationToken)
        {
            var result = await _xMlFuelStationService.ParceLoversExcelFile(file, cancellationToken);
            return Ok(result);
        }


        [HttpPost("ParceTaAndPetroStationFile")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> ParceTaAndPetroStationFile(IFormFile file,
          CancellationToken cancellationToken)
        {
            var result = await _xMlFuelStationService.ParceTaAndPetroStationFile(file, cancellationToken);
            return Ok(result);
        }

        [HttpPost("ParceTaAndPetroStationInfoFile")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> ParceTaAndPetroStationInfoFile(IFormFile file,
          CancellationToken cancellationToken)
        {
            var result = await _xMlFuelStationService.ParceTaAndPetroStationInfoFile(file, cancellationToken);
            return Ok(result);
        }

        [HttpPost("LoadInfo")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> LoadInfoCommand(IFormFile file,
          CancellationToken cancellationToken)
        {
            var request = new LoadInfoCommand(file);
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }


        [HttpPost("LoadPrice")]
        public async Task<ActionResult<IEnumerable<FuelStationDto>>> LoadPriceCommand(IFormFile file,
          CancellationToken cancellationToken)
        {
            var request = new LoadPriceCommand(file);
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("load-loves-stores")]
        public async Task<ActionResult<LovesApiResponseModel>> LoadLovesStores(CancellationToken cancellationToken)
        {
            var request = new LoadLovesStoresCommand();
            var result = await _mediator.Send(request, cancellationToken);
            
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }
            
            return Ok(result.Value);
        }




        //[HttpPost("parcePailot")]
        //public async Task<ActionResult<IEnumerable<FuelStationDto>>> Parceasdsad(CancellationToken cancellationToken)
        //{
        //    await _fuelStationsService.LoversePilotParce();
        //    return Ok();    
        //}




    }
}



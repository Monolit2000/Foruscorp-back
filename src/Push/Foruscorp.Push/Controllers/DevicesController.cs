using Foruscorp.Push.Features.Devices.RegisterDevice;
using Foruscorp.Push.Features.Devices.UpdateDeviceToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.Push.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ISender _mediator;

        public DevicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Guid>> Register([FromBody] RegisterDeviceCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (result.IsFailed)
                return Ok();
                //return BadRequest(result.Errors.FirstOrDefault()?.Message ?? "Failed to register device.");

            return CreatedAtAction(nameof(Register), new { id = result.Value}, result.Value);
        }

        [HttpPut("update-token")]
        public async Task<IActionResult> UpdateToken(
            [FromBody] UpdateDeviceTokenCommand cmdBody)
        {
            var cmd = cmdBody with { DeviceId = cmdBody.DeviceId };
            await _mediator.Send(cmd);
            return NoContent();
        }
    }
}

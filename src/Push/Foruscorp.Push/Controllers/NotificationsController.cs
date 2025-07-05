using Foruscorp.Push.Features.Notifications;
using Foruscorp.Push.Features.Notifications.CreateNotification;
using Foruscorp.Push.Features.Notifications.GetNotificationByUserId;
using Foruscorp.Push.Features.Notifications.GetNotificationStatus;
using Foruscorp.Push.Features.Notifications.SendNotification;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.Push.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController(
        ISender mediator) : ControllerBase
    {
 
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetByUser(Guid userId)
        {
            var q = new GetNotificationByUserIdQuery(userId);
            var list = await mediator.Send(q);
            return Ok(list);
        }

        [HttpPost("create-notification")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateNotificationCommand cmd)
        {
            var notificationId = await mediator.Send(cmd);
            return CreatedAtAction(nameof(GetStatus), new { notificationId }, notificationId);
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendNotificationCommand cmd)
        {
            await mediator.Send(cmd);
            return NoContent();
        }

        [HttpGet("{notificationId:guid}")]
        public async Task<ActionResult<NotificationDto>> GetStatus(Guid notificationId)
        {
            var q = new GetNotificationStatusQuery(notificationId);
            var dto = await mediator.Send(q);
            return Ok(dto);
        }
    }
}

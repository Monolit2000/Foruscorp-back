using Foruscorp.Push.Features.Notifications;
using Foruscorp.Push.Features.Notifications.CreateNotification;
using Foruscorp.Push.Features.Notifications.GetNotificationStatus;
using Foruscorp.Push.Features.Notifications.SendNotification;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.Push.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly ISender _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Создание нового уведомления и сразу добавление получателей.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateNotificationCommand cmd)
        {
            var notificationId = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(GetStatus), new { notificationId }, notificationId);
        }

        /// <summary>
        /// Запустить отправку уведомления всем pending–получателям.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendNotificationCommand cmd)
        {
            await _mediator.Send(cmd);
            return NoContent();
        }

        /// <summary>
        /// Получить статус уведомления и каждого получателя.
        /// </summary>
        [HttpGet("{notificationId:guid}")]
        public async Task<ActionResult<NotificationDto>> GetStatus(Guid notificationId)
        {
            var q = new GetNotificationStatusQuery(notificationId);
            var dto = await _mediator.Send(q);
            return Ok(dto);
        }
    }
}

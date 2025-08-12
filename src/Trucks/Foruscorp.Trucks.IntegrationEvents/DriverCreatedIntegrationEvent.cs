using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.IntegrationEvents
{
    public record DriverCreatedIntegrationEvent(
        Guid DriverId, 
        Guid UserId, 
        string Name,
        string Phone = null,
        string Email = null,
        string TelegramLink = null);
}

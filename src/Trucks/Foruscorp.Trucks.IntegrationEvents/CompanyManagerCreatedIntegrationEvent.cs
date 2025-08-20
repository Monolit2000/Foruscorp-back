using System;

namespace Foruscorp.Trucks.IntegrationEvents
{
    public record CompanyManagerCreatedIntegrationEvent(
        Guid CompanyManagerId, 
        Guid UserId, 
        Guid CompanyId,
        string Name,
        string Email,
        string CompanyName);
}

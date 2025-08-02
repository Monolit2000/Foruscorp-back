using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.IntegrationEvents
{
    public record CompanyCreatedIntegrationEvent(Guid CompanyId, string Name);
}

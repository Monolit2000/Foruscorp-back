using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.BuildingBlocks.Infastructure.EventBus
{
    public interface IEventBus
    {
        public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
           where T : class, IIntegrationEvent;
    }
}

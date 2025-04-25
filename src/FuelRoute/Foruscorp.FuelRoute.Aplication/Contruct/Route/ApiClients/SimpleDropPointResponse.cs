using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients
{
    public record class SimpleDropPointResponse(double latitude, double longitude);
}

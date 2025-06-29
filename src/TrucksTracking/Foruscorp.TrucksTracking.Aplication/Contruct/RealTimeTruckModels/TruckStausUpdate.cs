using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels
{
    public sealed record TruckStausUpdate(
       string TruckId,
       int Status);
}

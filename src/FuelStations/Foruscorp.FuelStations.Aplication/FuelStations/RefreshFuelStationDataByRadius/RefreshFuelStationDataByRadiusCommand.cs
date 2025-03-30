using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;

namespace Foruscorp.FuelStations.Aplication.FuelStations.RefreshFuelStationDataByRadius
{
    public class RefreshFuelStationDataByRadiusCommand : IRequest<Result>
    {
        public string AddressPiont { get; set; }
        public int Radius { get; set; }
    }
}

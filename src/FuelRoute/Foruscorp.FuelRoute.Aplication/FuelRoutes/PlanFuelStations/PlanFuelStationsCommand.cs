﻿using FluentResults;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;
using FuelStationDto = Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.FuelStationDto;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.PlanFuelStations
{
    public class PlanFuelStationsCommand : IRequest<Result<PlanFuelStationsByRoadsResponce>>
    { 
        public Guid RouteId { get; set; }
        public List<string> RouteSectionIds { get; set; }
        public List<RequiredStationDto> RequiredFuelStations { get; set; } = new List<RequiredStationDto>();    
        public double FinishFuel { get; set; } = 40.0;
        public List<string> FuelProviderNameList { get; set; } = new List<string>();

        public double Weight { get; set; } = 40000.0; // in Paunds
        public int CurrentFuel { get; set; } = 20;
    }
}

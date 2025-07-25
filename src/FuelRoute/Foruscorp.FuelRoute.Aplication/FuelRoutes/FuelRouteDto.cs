﻿
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute.CreateFuelRouteCommandHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes
{
    public class FuelRouteDto
    {
        public string RouteId { get; set; } 

        public List<RouteDto> RouteDtos { get; set; } = new List<RouteDto>();  
        
        public List<FuelStationDto> FuelStationDtos { get; set; } = new List<FuelStationDto>(); 
    }

    public class RouteDto
    {
        public string RouteSectionId { get; set; } 

        public List<List<double>> MapPoints { get; set; } = new List<List<double>>();

        public RouteInfo RouteInfo { get; set; } 
    }

}

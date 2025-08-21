using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class FuelStopStationPlanner
    {
        private readonly RouteAnalyzer _routeAnalyzer;
        private readonly FuelStopCalculator _fuelStopCalculator;
        private readonly StationSelector _stationSelector;
        private readonly RefuelingPlanner _refuelingPlanner;

        public FuelStopStationPlanner()
        {
            _routeAnalyzer = new RouteAnalyzer();
            _fuelStopCalculator = new FuelStopCalculator();
            _stationSelector = new StationSelector();
            _refuelingPlanner = new RefuelingPlanner();
        }

        public StopPlanInfo PlanStopsByStations(
            List<GeoPoint> route,
            List<FuelStation> stationsAlongRoute,
            double totalRouteDistanceKm,
            double fuelConsumptionPerKm,
            double currentFuelLiters,
            double tankCapacity,
            List<RequiredStationDto> requiredStops,
            double finishFuel,
            string roadSectionId = null)
        {
            // Валидация входных параметров
            ValidateInputParameters(finishFuel, tankCapacity, totalRouteDistanceKm);

            // Анализ маршрута и станций
            var routeAnalysis = _routeAnalyzer.AnalyzeRoute(route, stationsAlongRoute, totalRouteDistanceKm);

            // Планирование остановок
            var stopPlan = _refuelingPlanner.CreateStopPlan(
                routeAnalysis,
                new FuelPlanningParameters
                {
                    CurrentFuelLiters = currentFuelLiters,
                    TankCapacity = tankCapacity,
                    FuelConsumptionPerKm = fuelConsumptionPerKm,
                    FinishFuel = finishFuel,
                    RequiredStops = requiredStops ?? new List<RequiredStationDto>(),
                    RoadSectionId = roadSectionId ?? string.Empty
                });

            return new StopPlanInfo
            {
                StopPlan = stopPlan.Stops,
                Finish = stopPlan.FinishInfo
            };
        }

        private void ValidateInputParameters(double finishFuel, double tankCapacity, double totalRouteDistanceKm)
        {
            if (finishFuel < 0 || finishFuel > tankCapacity)
                throw new ArgumentException($"finishFuel must be between 0 and tankCapacity ({tankCapacity} liters).");

            if (totalRouteDistanceKm <= 0)
                throw new ArgumentException("totalRouteDistanceKm must be positive.");
        }
    }

    public class RouteAnalyzer
    {
        private const double SearchRadiusKm = 9.0;

        public RouteAnalysis AnalyzeRoute(List<GeoPoint> route, List<FuelStation> stations, double totalDistance)
        {
            var stationInfos = CreateStationInfos(route, stations);
            var endInfo = CreateEndInfo(totalDistance);
            stationInfos.Add(endInfo);

            return new RouteAnalysis
            {
                StationInfos = stationInfos,
                TotalDistanceKm = totalDistance
            };
        }

        private List<StationInfo> CreateStationInfos(List<GeoPoint> route, List<FuelStation> stations)
        {
            var stationInfos = new List<StationInfo>();

            foreach (var station in stations)
            {
                var forwardDistance = CalculateForwardDistance(route, station.Coordinates);
                if (forwardDistance < double.MaxValue)
                {
                    var priceInfo = GetCheapestFuelPrice(station);
                    stationInfos.Add(new StationInfo
                    {
                        Station = station,
                        ForwardDistanceKm = forwardDistance,
                        PricePerLiter = priceInfo
                    });
                }
            }

            return stationInfos.OrderBy(s => s.ForwardDistanceKm).ToList();
        }

        private double CalculateForwardDistance(List<GeoPoint> route, GeoPoint stationCoords)
        {
            double cumulativeDistance = 0.0;

            for (int i = 0; i < route.Count - 1; i++)
            {
                var segmentStart = route[i];
                var segmentEnd = route[i + 1];

                var segmentLength = GeoCalculator.CalculateHaversineDistance(segmentStart, segmentEnd);
                var distanceToSegment = GeoCalculator.DistanceFromPointToSegmentKm(stationCoords, segmentStart, segmentEnd);

                if (distanceToSegment <= SearchRadiusKm)
                {
                    var projectionDistance = GeoCalculator.DistanceAlongSegment(segmentStart, segmentEnd, stationCoords);
                    return cumulativeDistance + projectionDistance;
                }

                cumulativeDistance += segmentLength;
            }

            return double.MaxValue;
        }

        private double GetCheapestFuelPrice(FuelStation station)
        {
            return station.FuelPrices
                .Where(fp => fp.PriceAfterDiscount >= 0)
                .OrderBy(fp => fp.PriceAfterDiscount)
                .FirstOrDefault()?.PriceAfterDiscount ?? double.MaxValue;
        }

        private StationInfo CreateEndInfo(double totalDistance)
        {
            return new StationInfo
            {
                Station = null,
                ForwardDistanceKm = totalDistance,
                PricePerLiter = 0.0
            };
        }
    }

    public class RefuelingPlanner
    {
        private readonly FuelStopCalculator _fuelStopCalculator;
        private readonly StationSelector _stationSelector;

        public RefuelingPlanner()
        {
            _fuelStopCalculator = new FuelStopCalculator();
            _stationSelector = new StationSelector();
        }

        public StopPlanResult CreateStopPlan(RouteAnalysis routeAnalysis, FuelPlanningParameters parameters)
        {
            var stopPlan = new List<FuelStopPlan>();
            var usedStationIds = new HashSet<Guid>();
            var currentState = new FuelState
            {
                RemainingFuel = parameters.CurrentFuelLiters,
                PreviousKm = 0.0,
                IsFirstStop = true
            };

            // Обработка обязательных остановок
            var requiredStops = ProcessRequiredStops(routeAnalysis, parameters, usedStationIds, currentState);
            stopPlan.AddRange(requiredStops);

            // Планирование промежуточных остановок до конца маршрута
            var intermediateStops = PlanIntermediateStops(
                routeAnalysis,
                parameters,
                usedStationIds,
                currentState);
            stopPlan.AddRange(intermediateStops);

            // Корректировка последней остановки для достижения finishFuel
            if (stopPlan.Any())
            {
                AdjustLastStop(stopPlan, routeAnalysis.TotalDistanceKm, parameters);
            }

            // Рассчитываем финальное топливо с учетом finishFuel
            var finalFuel = CalculateFinalFuel(stopPlan, routeAnalysis.TotalDistanceKm, parameters);

            return new StopPlanResult
            {
                Stops = stopPlan,
                FinishInfo = new FinishInfo { RemainingFuelLiters = finalFuel }
            };
        }

        private List<FuelStopPlan> ProcessRequiredStops(
            RouteAnalysis routeAnalysis,
            FuelPlanningParameters parameters,
            HashSet<Guid> usedStationIds,
            FuelState currentState)
        {
            var requiredStops = new List<FuelStopPlan>();
            var requiredInfos = CreateRequiredStationInfos(routeAnalysis, parameters.RequiredStops);

            foreach (var requiredInfo in requiredInfos)
            {
                // Планирование остановок до обязательной
                var intermediateStops = _fuelStopCalculator.PlanStopsUntilTarget(
                    routeAnalysis.StationInfos,
                    requiredInfo.ForwardDistanceKm,
                    parameters,
                    usedStationIds,
                    currentState);

                requiredStops.AddRange(intermediateStops);

                // Обязательная остановка
                var requiredStop = CreateRequiredStop(requiredInfo, parameters, currentState);
                requiredStops.Add(requiredStop);
                usedStationIds.Add(requiredInfo.Station.Id);
            }

            return requiredStops;
        }

        private List<FuelStopPlan> PlanIntermediateStops(
            RouteAnalysis routeAnalysis,
            FuelPlanningParameters parameters,
            HashSet<Guid> usedStationIds,
            FuelState currentState)
        {
            return _fuelStopCalculator.PlanStopsUntilTarget(
                routeAnalysis.StationInfos,
                routeAnalysis.TotalDistanceKm,
                parameters,
                usedStationIds,
                currentState);
        }

        private List<RequiredStationInfo> CreateRequiredStationInfos(
            RouteAnalysis routeAnalysis,
            List<RequiredStationDto> requiredStops)
        {
            return requiredStops
                .Select(required => new RequiredStationInfo
                {
                    Station = routeAnalysis.StationInfos.FirstOrDefault(s => s.Station?.Id == required.StationId)?.Station,
                    RefillLiters = required.RefillLiters,
                    ForwardDistanceKm = routeAnalysis.StationInfos.FirstOrDefault(s => s.Station?.Id == required.StationId)?.ForwardDistanceKm ?? 0
                })
                .Where(r => r.Station != null)
                .OrderBy(r => r.ForwardDistanceKm)
                .ToList();
        }

        private FuelStopPlan CreateRequiredStop(
            RequiredStationInfo requiredInfo,
            FuelPlanningParameters parameters,
            FuelState currentState)
        {
            var allowedRefill = Math.Min(requiredInfo.RefillLiters, parameters.TankCapacity - currentState.RemainingFuel);



            var preRefuelFuel = currentState.RemainingFuel;
            currentState.RemainingFuel += allowedRefill;

            return new FuelStopPlan
            {
                Station = requiredInfo.Station!,
                StopAtKm = requiredInfo.ForwardDistanceKm,
                RefillLiters = allowedRefill,
                CurrentFuelLiters = preRefuelFuel,
                RoadSectionId = parameters.RoadSectionId
            };
        }

        private void AdjustLastStop(List<FuelStopPlan> stopPlan, double totalDistance, FuelPlanningParameters parameters)
        {
            var lastStop = stopPlan.Last();
            var fuelToFinish = (totalDistance - lastStop.StopAtKm) * parameters.FuelConsumptionPerKm;

            // Рассчитываем необходимое топливо для достижения finishFuel
            var requiredFuelAtFinish = fuelToFinish + parameters.FinishFuel;
            var requiredRefill = requiredFuelAtFinish - lastStop.CurrentFuelLiters;

            // Учитываем минимальный запас топлива (20% от бака) для безопасности
            var minimumReserve = parameters.TankCapacity * 0.20; // 20% от бака
            var requiredReserveRefill = Math.Max(0, minimumReserve - lastStop.CurrentFuelLiters);
            
            // Используем максимальное из требуемой дозаправки для finishFuel и минимального запаса
            requiredRefill = Math.Max(requiredRefill, requiredReserveRefill);

            // Ограничиваем максимальной вместимостью бака
            var maxRefill = parameters.TankCapacity - lastStop.CurrentFuelLiters;

            if (requiredRefill > 0 && requiredRefill <= maxRefill)
            {
                lastStop.RefillLiters = requiredRefill;
            }
            else
            {
                // Если дозаправка 0 или меньше, удаляем последнюю остановку
                stopPlan.RemoveAt(stopPlan.Count - 1);
            }
        }

        private double CalculateFinalFuel(List<FuelStopPlan> stopPlan, double totalDistance, FuelPlanningParameters parameters)
        {
            if (!stopPlan.Any())
            {
                // Если нет остановок, рассчитываем топливо от начального состояния
                var fuelUsed = totalDistance * parameters.FuelConsumptionPerKm;
                var finalFuel = parameters.CurrentFuelLiters - fuelUsed;
                return Math.Max(finalFuel, parameters.FinishFuel);
            }

            var lastStop = stopPlan.Last();
            var fuelAfterLastStop = lastStop.CurrentFuelLiters + lastStop.RefillLiters;
            var distanceToFinish = totalDistance - lastStop.StopAtKm;
            var fuelUsedToFinish = distanceToFinish * parameters.FuelConsumptionPerKm;
            var actualFinalFuel = fuelAfterLastStop - fuelUsedToFinish;

            // Возвращаем максимальное из фактического топлива и желаемого finishFuel
            return Math.Max(actualFinalFuel, parameters.FinishFuel);
        }
    }

    public class FuelStopCalculator
    {
        private const double MinStopDistanceKm = 200.0;
        private const double RefillIncrement = 5.0;

        public List<FuelStopPlan> PlanStopsUntilTarget(
            List<StationInfo> stationInfos,
            double targetKm,
            FuelPlanningParameters parameters,
            HashSet<Guid> usedStationIds,
            FuelState currentState)
        {
            var stops = new List<FuelStopPlan>();

            while (currentState.PreviousKm < targetKm)
            {
                var neededFuel = CalculateNeededFuel(targetKm, currentState.PreviousKm, parameters);

                if (currentState.RemainingFuel >= neededFuel)
                    break;

                var nextStop = FindNextStop(stationInfos, parameters, usedStationIds, currentState);
                if (nextStop == null)
                    break;

                var stopPlan = CreateStopPlan(nextStop, parameters, currentState);

                // Добавляем остановку только если дозаправка больше нуля
                if (stopPlan.RefillLiters > 0)
                {
                    stops.Add(stopPlan);
                    usedStationIds.Add(nextStop.Station!.Id);
                }
                else
                {
                    // Если дозаправка нулевая, просто проезжаем мимо станции
                    var distance = nextStop.ForwardDistanceKm - currentState.PreviousKm;
                    currentState.RemainingFuel -= distance * parameters.FuelConsumptionPerKm;
                    currentState.PreviousKm = nextStop.ForwardDistanceKm;
                }
            }

            // Проезжаем до цели
            var finalDistance = targetKm - currentState.PreviousKm;
            currentState.RemainingFuel -= finalDistance * parameters.FuelConsumptionPerKm;
            currentState.PreviousKm = targetKm;

            return stops;
        }

        private double CalculateNeededFuel(double targetKm, double currentKm, FuelPlanningParameters parameters)
        {
            var distanceFuel = (targetKm - currentKm) * parameters.FuelConsumptionPerKm;

            // Учитываем finishFuel только если это конечная точка маршрута
            var finishFuel = targetKm >= parameters.TotalDistanceKm ? parameters.FinishFuel : 0;

            // Добавляем минимальный запас топлива (20% от бака) для безопасности
            // Это гарантирует, что в баке будет не меньше 20% на момент доезда до заправки
            var minimumReserve = parameters.TankCapacity * 0.20; // 20% от бака

            return distanceFuel + finishFuel + minimumReserve;
        }

        private StationInfo? FindNextStop(
            List<StationInfo> stationInfos,
            FuelPlanningParameters parameters,
            HashSet<Guid> usedStationIds,
            FuelState currentState)
        {
            var maxDistanceWithoutRefuel = currentState.RemainingFuel / parameters.FuelConsumptionPerKm;
            var maxReachKm = currentState.PreviousKm + maxDistanceWithoutRefuel;

            var currentMinDistance = MinStopDistanceKm;
            
            while (currentMinDistance >= 0)
            {
                var candidates = stationInfos
                    .Where(si => IsValidCandidate(si, currentState.PreviousKm, maxReachKm, usedStationIds, maxDistanceWithoutRefuel, currentMinDistance))
                    .Where(si => WillHaveMinimumReserve(si, currentState, parameters))
                    .OrderBy(si => si.PricePerLiter) // Сначала ближайшие
                    //.ThenBy(si => si.PricePerLiter) // Затем по цене
                    .ToList();

                if (candidates.Any())
                {
                    return candidates.FirstOrDefault();
                }

                // Уменьшаем минимальное расстояние на 100 км
                currentMinDistance -= 50.0;
            }

            return null;
        }

        private bool WillHaveMinimumReserve(StationInfo stationInfo, FuelState currentState, FuelPlanningParameters parameters)
        {
            var distanceToStation = stationInfo.ForwardDistanceKm - currentState.PreviousKm;
            var fuelUsedToStation = distanceToStation * parameters.FuelConsumptionPerKm;
            var fuelAtStation = currentState.RemainingFuel - fuelUsedToStation;
            
            // Проверяем, что при приезде на станцию в баке будет не меньше 20%
            var minimumReserve = parameters.TankCapacity * 0.20;
            return fuelAtStation >= minimumReserve;
        }

        private bool IsValidCandidate(
            StationInfo stationInfo,
            double previousKm,
            double maxReachKm,
            HashSet<Guid> usedStationIds,
            double maxDistanceWithoutRefuel,
            double currentMinDistance)
        {

            if (!usedStationIds.Any())
                return true;

            if (stationInfo.Station == null || usedStationIds.Contains(stationInfo.Station.Id))
                return false;

            if (stationInfo.ForwardDistanceKm <= previousKm || stationInfo.ForwardDistanceKm > maxReachKm)
                return false;

            //return true;
            // Применяем текущее ограничение по минимальному расстоянию
            return stationInfo.ForwardDistanceKm - previousKm >= currentMinDistance;
        }

        private FuelStopPlan CreateStopPlan(
            StationInfo stationInfo,
            FuelPlanningParameters parameters,
            FuelState currentState)
        {
            var distance = stationInfo.ForwardDistanceKm - currentState.PreviousKm;
            currentState.RemainingFuel -= distance * parameters.FuelConsumptionPerKm;

            var preRefuelFuel = currentState.RemainingFuel;
            var isLastRefuel = stationInfo.ForwardDistanceKm + GetExtraRange(parameters) >= parameters.TotalDistanceKm;
            var effectiveCapacity = GetEffectiveCapacity(currentState.IsFirstStop, isLastRefuel, parameters.TankCapacity);
            var refillAmount = CalculateRefillAmount(effectiveCapacity, currentState.RemainingFuel, parameters, isLastRefuel, parameters.TotalDistanceKm - stationInfo.ForwardDistanceKm);

            currentState.RemainingFuel += refillAmount;
            currentState.PreviousKm = stationInfo.ForwardDistanceKm;
            currentState.IsFirstStop = false;

            return new FuelStopPlan
            {
                Station = stationInfo.Station!,
                StopAtKm = stationInfo.ForwardDistanceKm,
                RefillLiters = refillAmount,
                CurrentFuelLiters = preRefuelFuel,
                RoadSectionId = parameters.RoadSectionId
            };
        }

        private double GetExtraRange(FuelPlanningParameters parameters)
        {
            return parameters.TankCapacity / parameters.FuelConsumptionPerKm;
        }

        private double GetEffectiveCapacity(bool isFirstStop, bool isLastRefuel, double tankCapacity)
        {
            return tankCapacity;
        }

        private double CalculateRefillAmount(double effectiveCapacity, double remainingFuel, FuelPlanningParameters parameters, bool isLastRefuel, double distanceToFinish)
        {
            var freeSpace = effectiveCapacity - remainingFuel;
            var rawRefill = freeSpace;

            // Если это последняя остановка, учитываем finishFuel
            if (isLastRefuel)
            {
                var fuelNeededToFinish = distanceToFinish * parameters.FuelConsumptionPerKm;
                var requiredFuelAtFinish = fuelNeededToFinish + parameters.FinishFuel;
                var requiredRefill = requiredFuelAtFinish - remainingFuel;

                // Используем максимальное из обычной дозаправки и требуемой для finishFuel
                rawRefill = Math.Max(rawRefill, requiredRefill);
            }
            else
            {
                // Для промежуточных остановок учитываем минимальный запас топлива (20% от бака)
                var minimumReserve = parameters.TankCapacity * 0.20; // 20% от бака
                var requiredReserveRefill = Math.Max(0, minimumReserve - remainingFuel);
                
                // Используем максимальное из обычной дозаправки и требуемой для минимального запаса
                rawRefill = Math.Max(rawRefill, requiredReserveRefill);
            }

            var refill = Math.Floor(rawRefill / RefillIncrement) * RefillIncrement;

            if (refill == 0 && freeSpace >= RefillIncrement)
                refill = RefillIncrement;
            else if (refill == 0 && freeSpace < RefillIncrement)
                refill = freeSpace;



            return Math.Min(refill, freeSpace);
        }
    }

    public class StationSelector
    {
        public List<StationInfo> SelectStationsByPrice(List<StationInfo> stations, int count)
        {
            return stations
                .Where(s => s.PricePerLiter < double.MaxValue)
                .OrderBy(s => s.PricePerLiter)
                .Take(count)
                .ToList();
        }
    }

    // DTO классы для передачи данных
    public class RouteAnalysis
    {
        public List<StationInfo> StationInfos { get; set; } = new();
        public double TotalDistanceKm { get; set; }
    }

    public class FuelPlanningParameters
    {
        public double CurrentFuelLiters { get; set; }
        public double TankCapacity { get; set; }
        public double FuelConsumptionPerKm { get; set; }
        public double FinishFuel { get; set; }
        public double TotalDistanceKm { get; set; }
        public List<RequiredStationDto> RequiredStops { get; set; } = new();
        public string RoadSectionId { get; set; } = string.Empty;
    }

    public class FuelState
    {
        public double RemainingFuel { get; set; }
        public double PreviousKm { get; set; }
        public bool IsFirstStop { get; set; } = true;
    }

    public class RequiredStationInfo
    {
        public FuelStation? Station { get; set; }
        public double RefillLiters { get; set; }
        public double ForwardDistanceKm { get; set; }
    }

    public class StopPlanResult
    {
        public List<FuelStopPlan> Stops { get; set; } = new();
        public FinishInfo FinishInfo { get; set; } = new();
    }

    public class StationInfo
    {
        public FuelStation? Station { get; set; }
        public double ForwardDistanceKm { get; set; }
        public double PricePerLiter { get; set; }
    }
}

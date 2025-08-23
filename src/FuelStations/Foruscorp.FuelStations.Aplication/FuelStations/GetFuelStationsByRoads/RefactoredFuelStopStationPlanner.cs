using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Рефакторенный планировщик остановок с использованием рекурсии и чистой архитектуры
    /// </summary>
    public class RefactoredFuelStopStationPlanner
    {
        private readonly IRouteAnalyzer _routeAnalyzer;
        private readonly IRecursiveFuelPlanner _fuelPlanner;

        public RefactoredFuelStopStationPlanner(
            IRouteAnalyzer routeAnalyzer = null,
            IRecursiveFuelPlanner fuelPlanner = null)
        {
            _routeAnalyzer = routeAnalyzer ?? new NewRouteAnalyzer();
            _fuelPlanner = fuelPlanner ?? new RecursiveFuelPlanner();
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
            // Валидация
            ValidateInputParameters(finishFuel, tankCapacity, totalRouteDistanceKm);

            // Создаем контекст планирования
            var context = new FuelPlanningContext
            {
                Route = route,
                TotalDistanceKm = totalRouteDistanceKm,
                FuelConsumptionPerKm = fuelConsumptionPerKm,
                CurrentFuelLiters = currentFuelLiters,
                TankCapacity = tankCapacity,
                FinishFuel = finishFuel,
                RequiredStops = requiredStops ?? new List<RequiredStationDto>(),
                RoadSectionId = roadSectionId ?? string.Empty
            };

            // Анализируем маршрут
            var stationInfos = _routeAnalyzer.AnalyzeStations(route, stationsAlongRoute, totalRouteDistanceKm);

            // Рекурсивно планируем остановки
            var stops = _fuelPlanner.PlanStopsRecursively(stationInfos, context);

            return new StopPlanInfo
            {
                StopPlan = stops,
                Finish = CalculateFinishInfo(stops, context)
            };
        }

        private static void ValidateInputParameters(double finishFuel, double tankCapacity, double totalRouteDistanceKm)
        {
            if (finishFuel < 0 || finishFuel > tankCapacity)
                throw new ArgumentException($"finishFuel must be between 0 and tankCapacity ({tankCapacity} liters).");

            if (totalRouteDistanceKm <= 0)
                throw new ArgumentException("totalRouteDistanceKm must be positive.");
        }

        private static FinishInfo CalculateFinishInfo(List<FuelStopPlan> stops, FuelPlanningContext context)
        {
            if (!stops.Any())
            {
                var fuelUsed = context.TotalDistanceKm * context.FuelConsumptionPerKm;
                var finalFuelA = context.CurrentFuelLiters - fuelUsed;
                return new FinishInfo { RemainingFuelLiters = Math.Max(finalFuelA, context.FinishFuel) };
            }

            var lastStop = stops.Last();
            var fuelAfterLastStop = lastStop.CurrentFuelLiters + lastStop.RefillLiters;
            var distanceToFinish = context.TotalDistanceKm - lastStop.StopAtKm;
            var fuelUsedToFinish = distanceToFinish * context.FuelConsumptionPerKm;
            var finalFuel = fuelAfterLastStop - fuelUsedToFinish;

            return new FinishInfo { RemainingFuelLiters = Math.Max(finalFuel, context.FinishFuel) };
        }
    }

    #region Interfaces

    public interface IRouteAnalyzer
    {
        List<StationInfo> AnalyzeStations(List<GeoPoint> route, List<FuelStation> stations, double totalDistance);
    }

    public interface IRecursiveFuelPlanner
    {
        List<FuelStopPlan> PlanStopsRecursively(List<StationInfo> stations, FuelPlanningContext context);
    }

    public interface IStationSelector
    {
        StationInfo? SelectOptimalStation(List<StationInfo> candidates, NewFuelState currentState, FuelPlanningContext context);
    }

    public interface IRefillCalculator
    {
        double CalculateOptimalRefill(StationInfo station, NewFuelState currentState, FuelPlanningContext context, List<StationInfo> allStations);
    }

    #endregion

    #region Implementations

    public class NewRouteAnalyzer : IRouteAnalyzer
    {

        public List<StationInfo> AnalyzeStations(List<GeoPoint> route, List<FuelStation> stations, double totalDistance)
        {
            return stations
                .Select(station => CreateStationInfo(route, station))
                .Where(info => info != null)
                .OrderBy(info => info.ForwardDistanceKm)
                .ToList();
        }

        private StationInfo? CreateStationInfo(List<GeoPoint> route, FuelStation station)
        {
            var forwardDistance = CalculateForwardDistanceRecursively(route, station.Coordinates, 0, 0.0);
            
            if (forwardDistance >= double.MaxValue)
                return null;

            var price = GetCheapestFuelPrice(station);
            if (price <= 0)
                return null;

            return new StationInfo
            {
                Station = station,
                ForwardDistanceKm = forwardDistance,
                PricePerLiter = price
            };
        }

        /// <summary>
        /// РЕКУРСИЯ 1: Рекурсивный расчет расстояния до станции вдоль маршрута
        /// </summary>
        private double CalculateForwardDistanceRecursively(
            List<GeoPoint> route, 
            GeoPoint stationCoords, 
            int segmentIndex, 
            double cumulativeDistance)
        {
            // Базовый случай: достигли конца маршрута
            if (segmentIndex >= route.Count - 1)
                return double.MaxValue;

            var segmentStart = route[segmentIndex];
            var segmentEnd = route[segmentIndex + 1];
            var segmentLength = GeoCalculator.CalculateHaversineDistance(segmentStart, segmentEnd);
            var distanceToSegment = GeoCalculator.DistanceFromPointToSegmentKm(stationCoords, segmentStart, segmentEnd);

            // Если станция достаточно близко к текущему сегменту
            if (distanceToSegment <= FuelPlanningConfig.SearchRadiusKm)
            {
                var projectionDistance = GeoCalculator.DistanceAlongSegment(segmentStart, segmentEnd, stationCoords);
                return cumulativeDistance + projectionDistance;
            }

            // Рекурсивный вызов для следующего сегмента
            return CalculateForwardDistanceRecursively(
                route, 
                stationCoords, 
                segmentIndex + 1, 
                cumulativeDistance + segmentLength);
        }

        private double GetCheapestFuelPrice(FuelStation station)
        {
            return station.FuelPrices
                .Where(fp => fp.PriceAfterDiscount > 0)
                .OrderBy(fp => fp.PriceAfterDiscount)
                .FirstOrDefault()?.PriceAfterDiscount ?? 0;
        }
    }

    public class RecursiveFuelPlanner : IRecursiveFuelPlanner
    {
        private readonly IStationSelector _stationSelector;
        private readonly IRefillCalculator _refillCalculator;

        public RecursiveFuelPlanner(
            IStationSelector stationSelector = null,
            IRefillCalculator refillCalculator = null)
        {
            _stationSelector = stationSelector ?? new OptimalStationSelector();
            _refillCalculator = refillCalculator ?? new SmartRefillCalculator();
        }

        /// <summary>
        /// РЕКУРСИЯ 2: Рекурсивное планирование остановок
        /// </summary>
        public List<FuelStopPlan> PlanStopsRecursively(List<StationInfo> stations, FuelPlanningContext context)
        {
            var initialState = new NewFuelState
            {
                RemainingFuel = context.CurrentFuelLiters,
                CurrentPosition = 0.0,
                UsedStationIds = new HashSet<Guid>()
            };

            return PlanStopsRecursivelyInternal(stations, context, initialState, new List<FuelStopPlan>());
        }

        /// <summary>
        /// Внутренний рекурсивный метод для планирования остановок
        /// </summary>
        private List<FuelStopPlan> PlanStopsRecursivelyInternal(
            List<StationInfo> stations,
            FuelPlanningContext context,
            NewFuelState currentState,
            List<FuelStopPlan> currentPlan)
        {
            // Базовый случай 1: Можем доехать до финиша
            if (CanReachFinish(currentState, context))
                return currentPlan;

            // Базовый случай 2: Нет доступных станций
            var availableStations = GetAvailableStations(stations, currentState, context);
            if (!availableStations.Any())
                return currentPlan;

            // Выбираем оптимальную станцию
            var selectedStation = _stationSelector.SelectOptimalStation(availableStations, currentState, context);
            if (selectedStation == null)
                return currentPlan;

            // Создаем остановку
            var stop = CreateFuelStop(selectedStation, currentState, context, stations);
            var newPlan = new List<FuelStopPlan>(currentPlan) { stop };

            // Обновляем состояние
            var newState = UpdateState(currentState, selectedStation, stop);

            // Рекурсивный вызов для планирования следующих остановок
            return PlanStopsRecursivelyInternal(stations, context, newState, newPlan);
        }

        private bool CanReachFinish(NewFuelState state, FuelPlanningContext context)
        {
            var distanceToFinish = context.TotalDistanceKm - state.CurrentPosition;
            var fuelNeeded = distanceToFinish * context.FuelConsumptionPerKm + context.FinishFuel;
            return state.RemainingFuel >= fuelNeeded;
        }

        private List<StationInfo> GetAvailableStations(
            List<StationInfo> stations,
            NewFuelState currentState, 
            FuelPlanningContext context)
        {
            var maxReach = currentState.CurrentPosition + 
                          (currentState.RemainingFuel / context.FuelConsumptionPerKm);

            return stations
                .Where(s => s.Station != null &&
                           !currentState.UsedStationIds.Contains(s.Station.Id) &&
                           s.ForwardDistanceKm > currentState.CurrentPosition &&
                           s.ForwardDistanceKm <= maxReach &&
                           // Применяем ограничение минимального расстояния (кроме первой остановки)
                           (currentState.CurrentPosition == 0.0 || 
                            s.ForwardDistanceKm - currentState.CurrentPosition >= FuelPlanningConfig.MinStopDistanceKm))
                .ToList();
        }

        private FuelStopPlan CreateFuelStop(
            StationInfo station,
            NewFuelState currentState,
            FuelPlanningContext context,
            List<StationInfo> allStations)
        {
            var distanceToStation = station.ForwardDistanceKm - currentState.CurrentPosition;
            var fuelUsed = distanceToStation * context.FuelConsumptionPerKm;
            var fuelAtArrival = currentState.RemainingFuel - fuelUsed;

            var refillAmount = _refillCalculator.CalculateOptimalRefill(
                station, currentState, context, allStations);

            return new FuelStopPlan
            {
                Station = station.Station!,
                StopAtKm = station.ForwardDistanceKm,
                RefillLiters = refillAmount,
                CurrentFuelLiters = fuelAtArrival,
                RoadSectionId = context.RoadSectionId
            };
        }

        private NewFuelState UpdateState(NewFuelState currentState, StationInfo selectedStation, FuelStopPlan stop)
        {
            return new NewFuelState
            {
                RemainingFuel = stop.CurrentFuelLiters + stop.RefillLiters,
                CurrentPosition = selectedStation.ForwardDistanceKm,
                UsedStationIds = new HashSet<Guid>(currentState.UsedStationIds) 
                { 
                    selectedStation.Station!.Id 
                }
            };
        }
    }

    public class OptimalStationSelector : IStationSelector
    {
        /// <summary>
        /// РЕКУРСИЯ 3: Рекурсивный поиск оптимальной станции с использованием lookahead
        /// </summary>
        public StationInfo? SelectOptimalStation(
            List<StationInfo> candidates,
            NewFuelState currentState, 
            FuelPlanningContext context)
        {
            if (!candidates.Any())
                return null;

            // Для первой остановки используем специальную логику
            if (currentState.CurrentPosition == 0.0)
                return SelectFirstStationRecursively(candidates, 0);

            // Для последующих остановок ищем более дешевые станции
            return SelectCheapestStationRecursively(candidates, 0, null, double.MaxValue);
        }

        /// <summary>
        /// Рекурсивный выбор первой станции (ищем самую дешевую)
        /// </summary>
        private StationInfo? SelectFirstStationRecursively(List<StationInfo> candidates, int index)
        {
            // Базовый случай: проверили все кандидаты
            if (index >= candidates.Count)
                return null;

            var current = candidates[index];
            var next = SelectFirstStationRecursively(candidates, index + 1);

            // Если следующей нет или текущая дешевле
            if (next == null || current.PricePerLiter < next.PricePerLiter)
                return current;

            return next;
        }

        /// <summary>
        /// Рекурсивный поиск самой дешевой станции
        /// </summary>
        private StationInfo? SelectCheapestStationRecursively(
            List<StationInfo> candidates, 
            int index, 
            StationInfo? currentBest, 
            double bestPrice)
        {
            // Базовый случай: проверили все кандидаты
            if (index >= candidates.Count)
                return currentBest;

            var candidate = candidates[index];
            
            // Обновляем лучший вариант если нашли более дешевую станцию
            if (candidate.PricePerLiter < bestPrice)
            {
                return SelectCheapestStationRecursively(
                    candidates, index + 1, candidate, candidate.PricePerLiter);
            }

            // Продолжаем поиск с текущим лучшим вариантом
            return SelectCheapestStationRecursively(
                candidates, index + 1, currentBest, bestPrice);
        }
    }

    public class SmartRefillCalculator : IRefillCalculator
    {

        public double CalculateOptimalRefill(
            StationInfo station,
            NewFuelState currentState,
            FuelPlanningContext context,
            List<StationInfo> allStations)
        {
            var distanceToStation = station.ForwardDistanceKm - currentState.CurrentPosition;
            var fuelUsed = distanceToStation * context.FuelConsumptionPerKm;
            var fuelAtArrival = currentState.RemainingFuel - fuelUsed;

            // Если это последняя возможная остановка - заправляемся для финиша
            if (IsLastPossibleStop(station, context, allStations))
                return CalculateRefillForFinish(fuelAtArrival, station, context);

            // Ищем следующие станции рекурсивно
            var nextCheaperStation = FindNextCheaperStationRecursively(
                allStations, station, 0, context);

            if (nextCheaperStation != null)
            {
                // Заправляемся минимально для доезда до более дешевой станции
                return CalculateMinimalRefillToNextStation(
                    fuelAtArrival, station, nextCheaperStation, context);
            }

            // Заправляемся до полного бака
            return Math.Max(context.TankCapacity - fuelAtArrival, FuelPlanningConfig.MinRefillLiters);
        }

        /// <summary>
        /// РЕКУРСИЯ 4: Рекурсивный поиск следующей более дешевой станции
        /// </summary>
        private StationInfo? FindNextCheaperStationRecursively(
            List<StationInfo> allStations,
            StationInfo currentStation,
            int index,
            FuelPlanningContext context)
        {
            // Базовый случай: проверили все станции
            if (index >= allStations.Count)
                return null;

            var candidate = allStations[index];

            // Проверяем условия для кандидата
            if (IsValidNextStation(candidate, currentStation, context))
            {
                var remaining = FindNextCheaperStationRecursively(
                    allStations, currentStation, index + 1, context);

                // Возвращаем более дешевую из найденных
                if (remaining == null || candidate.PricePerLiter < remaining.PricePerLiter)
                    return candidate;
                
                return remaining;
            }

            // Продолжаем поиск
            return FindNextCheaperStationRecursively(
                allStations, currentStation, index + 1, context);
        }

        private bool IsValidNextStation(
            StationInfo candidate,
            StationInfo currentStation,
            FuelPlanningContext context)
        {
            // Должна быть дальше по маршруту
            if (candidate.ForwardDistanceKm <= currentStation.ForwardDistanceKm)
                return false;

            // Должна быть дешевле
            if (candidate.PricePerLiter >= currentStation.PricePerLiter)
                return false;

            // Должна быть в пределах досягаемости с полным баком
            var maxRange = context.TankCapacity / context.FuelConsumptionPerKm;
            if (candidate.ForwardDistanceKm > currentStation.ForwardDistanceKm + maxRange)
                return false;

            // Применяем ограничение минимального расстояния
            var distanceBetweenStations = candidate.ForwardDistanceKm - currentStation.ForwardDistanceKm;
            if (distanceBetweenStations < FuelPlanningConfig.MinStopDistanceKm)
                return false;

            return true;
        }

        private bool IsLastPossibleStop(
            StationInfo station,
            FuelPlanningContext context,
            List<StationInfo> allStations)
        {
            var maxRange = context.TankCapacity / context.FuelConsumptionPerKm;
            var maxReach = station.ForwardDistanceKm + maxRange;

            return !allStations.Any(s => 
                s.ForwardDistanceKm > station.ForwardDistanceKm && 
                s.ForwardDistanceKm <= maxReach);
        }

        private double CalculateRefillForFinish(
            double fuelAtArrival,
            StationInfo station,
            FuelPlanningContext context)
        {
            var distanceToFinish = context.TotalDistanceKm - station.ForwardDistanceKm;
            var fuelNeededToFinish = distanceToFinish * context.FuelConsumptionPerKm;
            var requiredRefill = fuelNeededToFinish + context.FinishFuel - fuelAtArrival;

            return Math.Max(Math.Min(requiredRefill, context.TankCapacity - fuelAtArrival), 0);
        }

        private double CalculateMinimalRefillToNextStation(
            double fuelAtArrival,
            StationInfo currentStation,
            StationInfo nextStation,
            FuelPlanningContext context)
        {
            var distanceToNext = nextStation.ForwardDistanceKm - currentStation.ForwardDistanceKm;
            var fuelNeededToNext = distanceToNext * context.FuelConsumptionPerKm;
            var minReserve = context.TankCapacity * FuelPlanningConfig.MinReserveFactor;
            var requiredRefill = fuelNeededToNext + minReserve - fuelAtArrival;

            var maxRefill = context.TankCapacity - fuelAtArrival;
            var refill = Math.Min(Math.Max(requiredRefill, FuelPlanningConfig.MinRefillLiters), maxRefill);

            return Math.Max(refill, 0);
        }
    }

    #endregion

    #region Data Classes

    /// <summary>
    /// Конфигурация ограничений для планирования маршрута
    /// </summary>
    public static class FuelPlanningConfig
    {
        /// <summary>
        /// Минимальное расстояние между заправками (400 км)
        /// </summary>
        public const double MinStopDistanceKm = 400.0;
        
        /// <summary>
        /// Минимальный запас топлива (20% от бака)
        /// </summary>
        public const double MinReserveFactor = 0.20;
        
        /// <summary>
        /// Минимальное количество дозаправки (20 литров)
        /// </summary>
        public const double MinRefillLiters = 20.0;
        
        /// <summary>
        /// Радиус поиска заправок вдоль маршрута (9 км)
        /// </summary>
        public const double SearchRadiusKm = 9.0;
    }

    public class FuelPlanningContext
    {
        public List<GeoPoint> Route { get; set; } = new();
        public double TotalDistanceKm { get; set; }
        public double FuelConsumptionPerKm { get; set; }
        public double CurrentFuelLiters { get; set; }
        public double TankCapacity { get; set; }
        public double FinishFuel { get; set; }
        public List<RequiredStationDto> RequiredStops { get; set; } = new();
        public string RoadSectionId { get; set; } = string.Empty;
    }

    public class NewFuelState
    {
        public double RemainingFuel { get; set; }
        public double CurrentPosition { get; set; }
        public HashSet<Guid> UsedStationIds { get; set; } = new();
    }

    #endregion
}

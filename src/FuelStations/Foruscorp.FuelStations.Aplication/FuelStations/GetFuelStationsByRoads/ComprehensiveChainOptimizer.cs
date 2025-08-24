using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Комплексный оптимизатор цепочек заправок с полным перебором всех возможных вариантов
    /// Учитывает цену топлива, количество дозаправки, расстояния и выбирает самую выгодную цепочку
    /// </summary>
    public class ComprehensiveChainOptimizer
    {
        private readonly IChainCostCalculator _costCalculator;
        private readonly IChainValidator _validator;
        
        // 🔧 Настройки оптимизации
        public int MaxCheapStationsPerStep { get; set; } = 17;  // Максимум дешевых станций на каждом шаге
        public bool EnableCheapStationOptimization { get; set; } = true;  // Включить/выключить оптимизацию

        public ComprehensiveChainOptimizer(
            IChainCostCalculator costCalculator = null,
            IChainValidator validator = null)
        {
            _costCalculator = costCalculator ?? new SmartChainCostCalculator();
            _validator = validator ?? new ComprehensiveChainValidator();
        }

        /// <summary>
        /// Находит оптимальную цепочку заправок среди всех возможных вариантов
        /// </summary>
        public StopPlanInfo FindOptimalChainComprehensive(
            List<GeoPoint> route,
            List<FuelStation> stations, 
            double totalDistanceKm,
            double fuelConsumptionPerKm,
            double currentFuelLiters,
            double tankCapacity,
            List<RequiredStationDto> requiredStops,
            double finishFuel, string RoadSectionId)
        {
            var context = new FuelPlanningContext
            {
                Route = route,
                TotalDistanceKm = totalDistanceKm,
                FuelConsumptionPerKm = fuelConsumptionPerKm,
                CurrentFuelLiters = currentFuelLiters,
                TankCapacity = tankCapacity,
                FinishFuel = finishFuel,
                RequiredStops = requiredStops,
                RoadSectionId = RoadSectionId
            };

            // Создаем информацию о станциях с расстояниями
            var stationInfosF = CreateStationInfos(stations, route);

            var stationInfos = stationInfosF
                .Where(si => si.PricePerLiter != double.MaxValue)
                .Where(si => si.PricePerLiter > 0).ToList();

            Console.WriteLine($"🔄 Начинаем комплексный анализ {stationInfos.Count} станций...");

            // Генерируем все возможные цепочки
            var allChains = GenerateAllPossibleChains(stationInfos, context);
            Console.WriteLine($"📊 Сгенерировано {allChains.Count} возможных цепочек");

            // Фильтруем валидные цепочки
            var validChains = FilterValidChains(allChains, context);
            Console.WriteLine($"✅ Валидных цепочек: {validChains.Count}");

            if (!validChains.Any())
            {
                Console.WriteLine("❌ Не найдено валидных цепочек!");
                return new StopPlanInfo 
                { 
                    StopPlan = new List<FuelStopPlan>(),
                    Finish = CreateFinishInfo(context)
                };
            }

            // Вычисляем стоимость для каждой валидной цепочки
            var chainCosts = CalculateChainCosts(validChains, context);

            // Выбираем оптимальную цепочку
            var optimalChain = SelectOptimalChain(chainCosts);
            Console.WriteLine($"🎯 Выбрана оптимальная цепочка с {optimalChain.Chain.Stations.Count} остановками");

            // Преобразуем в результат
            return ConvertToStopPlanInfo(optimalChain, context);
        }

        /// <summary>
        /// Генерирует все возможные цепочки заправок
        /// </summary>
        private List<FuelChain> GenerateAllPossibleChains(
            List<StationInfo> stations, 
            FuelPlanningContext context)
        {
            var allChains = new List<FuelChain>();

            // 1. Цепочка без остановок (если возможно дойти до финиша)
            var noStopsChain = new FuelChain { Stations = new List<StationInfo>() };
            allChains.Add(noStopsChain);

            // 2. Генерируем цепочки разной длины (от 1 до максимальной разумной длины)
            var maxChainLength = Math.Min(stations.Count, 10); // Ограничиваем для производительности

            for (int chainLength = 1; chainLength <= maxChainLength; chainLength++)
            {
                var chainsOfLength = GenerateChainsOfLength(stations, chainLength, context);

                var newasdsad = chainsOfLength.OrderBy(c => c.Stations.First().PricePerLiter).ToList();

                allChains.AddRange(chainsOfLength);
                
                Console.WriteLine($"   Длина {chainLength}: {chainsOfLength.Count} цепочек");
                
                // Если цепочек становится слишком много, прерываем
                if (allChains.Count > 1000000) // Ограничение для производительности
                {
                    Console.WriteLine($"⚠️ Достигнуто ограничение на количество цепочек: {allChains.Count}");
                    break;
                }
            }

            return allChains;
        }

        /// <summary>
        /// Генерирует все цепочки заданной длины
        /// </summary>
        private List<FuelChain> GenerateChainsOfLength(
            List<StationInfo> stations, 
            int length, 
            FuelPlanningContext context)
        {
            var chains = new List<FuelChain>();

            // Рекурсивно генерируем комбинации
            GenerateCombinationsRecursive(
                stations, 
                length, 
                new List<StationInfo>(), 
                0, 
                chains, 
                context);

            return chains;
        }

        /// <summary>
        /// Рекурсивная генерация комбинаций станций с оптимизацией по дешевым станциям
        /// </summary>
        private void GenerateCombinationsRecursive(
            List<StationInfo> allStations,
            int targetLength,
            List<StationInfo> currentChain,
            int startIndex,
            List<FuelChain> result,
            FuelPlanningContext context)
        {
            // Базовый случай: достигли нужной длины
            if (currentChain.Count == targetLength)
            {
                // Проверяем, что цепочка упорядочена по расстоянию
                if (IsChainOrderedByDistance(currentChain))
                {
                    result.Add(new FuelChain { Stations = new List<StationInfo>(currentChain) });
                }
                return;
            }

            // 🚀 ОПТИМИЗАЦИЯ: Находим самые дешевые достижимые станции на полном баке
            List<StationInfo> stationsToProcess;
            
            if (EnableCheapStationOptimization)
            {
                stationsToProcess = GetAffordableCheapestStations(
                    allStations, currentChain, startIndex, context, MaxCheapStationsPerStep);
            }
            else
            {
                // Классический подход - все станции
                stationsToProcess = allStations
                    .Skip(startIndex)
                    .Where(station => CanAddStationToChain(currentChain, station, context))
                    .ToList();
            }

            //Console.WriteLine($"     Найдено {stationsToProcess.Count} доступных станций из {allStations.Count - startIndex}");

            // Рекурсивно добавляем только отобранные станции
            foreach (var station in stationsToProcess)
            {
                currentChain.Add(station);
                
                // Находим новый startIndex для следующей рекурсии
                var nextStartIndex = allStations.FindIndex(s => s.ForwardDistanceKm > station.ForwardDistanceKm);
                if (nextStartIndex == -1) nextStartIndex = allStations.Count;
                
                GenerateCombinationsRecursive(allStations, targetLength, currentChain, nextStartIndex, result, context);
                currentChain.RemoveAt(currentChain.Count - 1); 
            }
        }

        /// <summary>
        /// 💡 НОВЫЙ МЕТОД: Находит самые дешевые станции, до которых трак может доехать на полном баке
        /// </summary>
        private List<StationInfo> GetAffordableCheapestStations(
            List<StationInfo> allStations,
            List<StationInfo> currentChain,
            int startIndex,
            FuelPlanningContext context,
            int maxCheapStations)
        {
            // Определяем текущую позицию
            var currentPosition = currentChain.Any() ? currentChain.Last().ForwardDistanceKm : 0.0;
            var isFirstStation = !currentChain.Any();
            var currentFuelPercentage = GetCurrentFuelPercentage(currentChain, context);
            // Рассчитываем максимальное расстояние на полном баке
            var maxRangeOnFullTank = context.TankCapacity / context.FuelConsumptionPerKm;
            var maxReachableDistance = currentPosition + maxRangeOnFullTank;

            //Console.WriteLine($"       Текущая позиция: {currentPosition:F0}км");
            //Console.WriteLine($"       Запас хода на полном баке: {maxRangeOnFullTank:F0}км");
            //Console.WriteLine($"       Максимально достижимая дистанция: {maxReachableDistance:F0}км");

            // Фильтруем станции по достижимости на полном баке

            var reachableStations = allStations
                .Skip(startIndex)
                .Where(station => 
                {
                    // Базовые проверки
                    if (station.ForwardDistanceKm <= currentPosition) return false;
                    if (station.ForwardDistanceKm > maxReachableDistance) return false;

                    if (isFirstStation)
                    {

                        var distance = station.ForwardDistanceKm - currentPosition;
                        var fuelUsed = distance * context.FuelConsumptionPerKm;
                        var fuelAtArrival = context.CurrentFuelLiters - fuelUsed;

                        var minReserve = context.TankCapacity * FuelPlanningConfig.MinReserveFactor;

                        if (fuelAtArrival < minReserve)
                            return false;

                        if (station.ForwardDistanceKm > (currentFuelPercentage * context.TankCapacity) / context.FuelConsumptionPerKm) return false;
                    }

                    // Проверяем, что станция подходит для добавления в цепочку
                    return CanAddStationToChain(currentChain, station, context);
                })
                .ToList();

            //Console.WriteLine($"       Достижимых станций: {reachableStations.Count}");

            if (!reachableStations.Any())
            {
                //Console.WriteLine($"       ⚠️ Нет достижимых станций!");
                return new List<StationInfo>();
            }

            // Выбираем самые дешевые станции из достижимых
            var cheapestStations = reachableStations
                .OrderBy(s => s.PricePerLiter)                    // Сортируем по цене
                .ThenBy(s => s.ForwardDistanceKm)                 // При равной цене - ближайшие
                .Take(maxCheapStations)                           // Берем только 16 самых дешевых
                .OrderBy(s => s.ForwardDistanceKm)                // Сортируем обратно по расстоянию
                .ToList();

            //Console.WriteLine($"       Отобрано {cheapestStations.Count} самых дешевых:");
            //foreach (var station in cheapestStations.Take(5)) // Показываем первые 5
            //{
            //    Console.WriteLine($"         • {station.Station?.ProviderName}: {station.ForwardDistanceKm:F0}км, ${station.PricePerLiter:F2}/л");
            //}
            //if (cheapestStations.Count > 5)
            //{
            //    Console.WriteLine($"         ... и ещё {cheapestStations.Count - 5} станций");
            //}

            return cheapestStations;
        }

        private double GetCurrentFuelPercentage(List<StationInfo> currentChain, FuelPlanningContext context)
        {
            var currentFuel = context.CurrentFuelLiters;
            var currentPosition = 0.0;

            // Симулируем расход топлива по цепочке
            foreach (var station in currentChain)
            {
                var distance = station.ForwardDistanceKm - currentPosition;
                var fuelUsed = distance * context.FuelConsumptionPerKm;
                currentFuel -= fuelUsed;

                // Добавляем заправку (упрощенный расчет - до полного бака)
                currentFuel = context.TankCapacity;
                currentPosition = station.ForwardDistanceKm;
            }

            // Если нет цепочки, используем текущее топливо
            return currentFuel / context.TankCapacity;
        }

        /// <summary>
        /// Проверяет, можно ли добавить станцию в цепочку
        /// </summary>
        private bool CanAddStationToChain(
            List<StationInfo> currentChain, 
            StationInfo station, 
            FuelPlanningContext context)
        {
            // Проверяем упорядоченность по расстоянию
            if (currentChain.Any() && station.ForwardDistanceKm <= currentChain.Last().ForwardDistanceKm)
                return false;

            // Проверяем минимальное расстояние между станциями
            if (currentChain.Any())
            {
                var lastStation = currentChain.Last();
                var distance = station.ForwardDistanceKm - lastStation.ForwardDistanceKm;
                if (distance < FuelPlanningConfig.MinStopDistanceKm)
                    return false;
            }
            else
            {
                // Проверяем минимальное расстояние от старта (кроме первой остановки)
                if (station.ForwardDistanceKm < 50) // Минимум 50км от старта
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, упорядочена ли цепочка по расстоянию
        /// </summary>
        private bool IsChainOrderedByDistance(List<StationInfo> chain)
        {
            for (int i = 1; i < chain.Count; i++)
            {
                if (chain[i].ForwardDistanceKm <= chain[i-1].ForwardDistanceKm)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Фильтрует валидные цепочки
        /// </summary>
        private List<FuelChain> FilterValidChains(
            List<FuelChain> allChains, 
            FuelPlanningContext context)
        {
            var validChains = new List<FuelChain>();

            foreach (var chain in allChains)
            {
                if (_validator.IsChainValid(chain, context))
                {
                    validChains.Add(chain);
                }
            }

            return validChains;
        }

        /// <summary>
        /// Вычисляет стоимость всех цепочек
        /// </summary>
        private List<ChainCostInfo> CalculateChainCosts(
            List<FuelChain> validChains, 
            FuelPlanningContext context)
        {
            var chainCosts = new List<ChainCostInfo>();

            foreach (var chain in validChains)
            {
                var cost = _costCalculator.CalculateChainCost(chain, context);
                chainCosts.Add(new ChainCostInfo 
                { 
                    Chain = chain, 
                    Cost = cost 
                });
            }

            return chainCosts.OrderBy(cc => cc.Cost.TotalScore).ToList();
        }

        /// <summary>
        /// Выбирает оптимальную цепочку
        /// </summary>
        private ChainCostInfo SelectOptimalChain(List<ChainCostInfo> chainCosts)
        {
            if (!chainCosts.Any())
                throw new InvalidOperationException("Нет доступных цепочек для выбора");

            var dsfsdf = chainCosts.OrderBy(c => c.Cost.TotalFuelCost);

            var optimal = dsfsdf.First(); // Уже отсортированы по TotalScore

            Console.WriteLine($"🏆 ОПТИМАЛЬНАЯ ЦЕПОЧКА:");
            Console.WriteLine($"   Остановок: {optimal.Chain.Stations.Count}");
            Console.WriteLine($"   Общая стоимость топлива: ${optimal.Cost.TotalFuelCost:F2}");
            Console.WriteLine($"   Общий балл: {optimal.Cost.TotalScore:F2}");
            Console.WriteLine($"   Эффективность: {optimal.Cost.EfficiencyScore:F2}");

            return optimal;
        }

        /// <summary>
        /// Преобразует оптимальную цепочку в результат
        /// </summary>
        private StopPlanInfo ConvertToStopPlanInfo(
            ChainCostInfo optimalChain, 
            FuelPlanningContext context)
        {
            var stops = new List<FuelStopPlan>();
            var currentFuel = context.CurrentFuelLiters;
            var currentPosition = 0.0;

            foreach (var station in optimalChain.Chain.Stations)
            {
                // Рассчитываем расход топлива до станции
                var distance = station.ForwardDistanceKm - currentPosition;
                var fuelUsed = distance * context.FuelConsumptionPerKm;
                var fuelAtArrival = currentFuel - fuelUsed;

                // Рассчитываем оптимальную дозаправку
                var refillAmount = _costCalculator.CalculateOptimalRefillAmount(
                    station, fuelAtArrival, optimalChain.Chain.Stations, context);

                var stop = new FuelStopPlan
                {
                    Station = station.Station!,
                    StopAtKm = station.ForwardDistanceKm,
                    RefillLiters = refillAmount,
                    CurrentFuelLiters = fuelAtArrival,
                    RoadSectionId = context.RoadSectionId
                };

                stops.Add(stop);

                // Обновляем состояние
                currentFuel = fuelAtArrival + refillAmount;
                currentPosition = station.ForwardDistanceKm;
            }

            return new StopPlanInfo
            {
                StopPlan = stops,
                Finish = CreateFinishInfo(context)
            };
        }

        #region Helper Methods

        private List<StationInfo> CreateStationInfos(List<FuelStation> stations, List<GeoPoint> route)
        {
            var routeAnalyzer = new NewRouteAnalyzer();
            return stations.Select(station => new StationInfo
            {
                Station = station,
                ForwardDistanceKm = routeAnalyzer.CalculateForwardDistanceRecursively(route, station.Coordinates, 0, 0),
                PricePerLiter = GetBestPrice(station)
            }).Where(si => si.ForwardDistanceKm > 0)
              .OrderBy(si => si.ForwardDistanceKm)
              .ToList();
        }

        private double GetBestPrice(FuelStation station)
        {
            return station.FuelPrices
                .Where(fp => fp.PriceAfterDiscount > 0)
                .OrderBy(fp => fp.PriceAfterDiscount)
                .FirstOrDefault()?.PriceAfterDiscount ?? 0;
        }

        private FinishInfo CreateFinishInfo(FuelPlanningContext context)
        {
            return new FinishInfo
            {
                //TotalDistanceKm = context.TotalDistanceKm,
                RemainingFuelLiters = context.FinishFuel
            };
        }

        #endregion
    }

    /// <summary>
    /// Представляет цепочку заправочных станций
    /// </summary>
    public class FuelChain
    {
        public List<StationInfo> Stations { get; set; } = new List<StationInfo>();
    }

    /// <summary>
    /// Информация о стоимости цепочки
    /// </summary>
    public class ChainCostInfo
    {
        public FuelChain Chain { get; set; } = new FuelChain();
        public ChainCost Cost { get; set; } = new ChainCost();
    }

    /// <summary>
    /// Детальная стоимость цепочки
    /// </summary>
    public class ChainCost
    {
        public double TotalFuelCost { get; set; }        // Общая стоимость топлива
        public double TotalRefillAmount { get; set; }    // Общее количество дозаправки
        public double TotalDistance { get; set; }        // Общее расстояние между остановками
        public double EfficiencyScore { get; set; }      // Оценка эффективности
        public double TotalScore { get; set; }           // Итоговый балл для сравнения
        public List<StopCost> StopCosts { get; set; } = new List<StopCost>(); // Детали по каждой остановке
    }

    /// <summary>
    /// Стоимость отдельной остановки
    /// </summary>
    public class StopCost
    {
        public StationInfo Station { get; set; } = new StationInfo();
        public double RefillAmount { get; set; }
        public double RefillCost { get; set; }
        public double DistanceFromPrevious { get; set; }
        public double FuelEfficiency { get; set; }
    }
}

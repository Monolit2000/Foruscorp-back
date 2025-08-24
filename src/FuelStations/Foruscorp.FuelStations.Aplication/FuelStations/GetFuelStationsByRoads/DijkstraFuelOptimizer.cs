using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Оптимизатор маршрутов заправок на основе алгоритма Дейкстры
    /// Сложность: O((V + E) log V) где V = узлы, E = рёбра
    /// Значительно эффективнее полного перебора O(2^n × n!)
    /// </summary>
    public class DijkstraFuelOptimizer
    {
        private readonly IChainCostCalculator _costCalculator;
        private readonly IChainValidator _validator;
        
        // 🎯 Настройки производительности
        public double MaxReachDistanceKm { get; set; } = 2250; // Максимальная достижимость на полном баке
        public int MaxGraphNodes { get; set; } = 10000; // Лимит узлов графа для производительности

        public DijkstraFuelOptimizer(
            IChainCostCalculator costCalculator = null,
            IChainValidator validator = null)
        {
            _costCalculator = costCalculator ?? new SmartChainCostCalculator();
            _validator = validator ?? new ComprehensiveChainValidator();
        }

        /// <summary>
        /// Находит оптимальный маршрут заправок используя алгоритм Дейкстры
        /// </summary>
        public StopPlanInfo FindOptimalRoute(
            List<GeoPoint> route,
            List<FuelStation> stations,
            double totalDistanceKm,
            double fuelConsumptionPerKm,
            double currentFuelLiters,
            double tankCapacity,
            List<RequiredStationDto> requiredStops,
            double finishFuel)
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
                RoadSectionId = "dijkstra-optimization"
            };

            Console.WriteLine($"🗺️ Начинаем построение графа для {stations.Count} станций...");

            // 1. Строим граф заправочных станций
            var graph = BuildFuelStationGraph(stations, route, context);
            Console.WriteLine($"📊 Граф построен: {graph.Nodes.Count} узлов, {graph.Edges.Count} рёбер");

            // 1.5. Проверяем возможность прямого маршрута без заправок
            var directRouteCheck = CheckDirectRouteToFinish(context);
            if (directRouteCheck.IsPossible)
            {
                Console.WriteLine("🎯 Прямой маршрут возможен без заправок!");
                return new StopPlanInfo 
                { 
                    StopPlan = new List<FuelStopPlan>(),
                    Finish = CreateFinishInfo(context, directRouteCheck.FuelAtFinish)
                };
            }

            // 2. Запускаем алгоритм Дейкстры
            var optimalPath = RunDijkstraAlgorithm(graph, context);
            
            if (optimalPath == null || !optimalPath.IsValid)
            {
                Console.WriteLine("❌ Оптимальный путь не найден!");
                return new StopPlanInfo 
                { 
                    StopPlan = new List<FuelStopPlan>(),
                    Finish = CreateFinishInfo(context)
                };
            }

            Console.WriteLine($"🎯 Найден оптимальный путь: {optimalPath.Stations.Count} остановок, стоимость: ${optimalPath.TotalCost:F2}");

            // 3. Преобразуем в результат
            return ConvertPathToStopPlanInfo(optimalPath, context);
        }

        /// <summary>
        /// Строит граф заправочных станций с весами рёбер
        /// </summary>
        private FuelStationGraph BuildFuelStationGraph(
            List<FuelStation> stations, 
            List<GeoPoint> route,
            FuelPlanningContext context)
        {
            var graph = new FuelStationGraph();

            // Создаем информацию о станциях
            var stationInfos = CreateStationInfos(stations, route)
                .Where(si => si.PricePerLiter > 0 && si.PricePerLiter != double.MaxValue)
                .OrderBy(si => si.ForwardDistanceKm)
                .ToList();

            // 1. Создаем стартовый узел
            var startNode = new FuelGraphNode
            {
                Id = "START",
                NodeType = GraphNodeType.Start,
                Position = 0,
                FuelLevel = context.CurrentFuelLiters,
                StationInfo = null
            };
            graph.AddNode(startNode);

            // 2. Создаем финишный узел  
            var finishNode = new FuelGraphNode
            {
                Id = "FINISH",
                NodeType = GraphNodeType.Finish,
                Position = context.TotalDistanceKm,
                FuelLevel = context.FinishFuel,
                StationInfo = null
            };
            graph.AddNode(finishNode);

            // 3. Создаем узлы для каждой станции с разными уровнями заправки
            foreach (var station in stationInfos)
            {
                CreateStationNodes(graph, station, context);
            }

            // 4. Создаем рёбра между узлами
            CreateGraphEdges(graph, context);

            return graph;
        }

        /// <summary>
        /// Создает узлы для станции с разными уровнями заправки
        /// </summary>
        private void CreateStationNodes(FuelStationGraph graph, StationInfo station, FuelPlanningContext context)
        {
            // Создаем узлы для разных уровней дозаправки (25%, 50%, 75%, 100%)
            var refillLevels = new[] { 0.25, 0.5, 0.75, 1.0 };
            
            foreach (var level in refillLevels)
            {
                var nodeId = $"STATION_{station.Station.Id}_{level:P0}";
                var estimatedRefill = context.TankCapacity * level;
                
                var node = new FuelGraphNode
                {
                    Id = nodeId,
                    NodeType = GraphNodeType.Station,
                    Position = station.ForwardDistanceKm,
                    FuelLevel = estimatedRefill, // Это будет уточнено при построении рёбер
                    StationInfo = station,
                    RefillLevel = level
                };
                
                graph.AddNode(node);
            }
        }

        /// <summary>
        /// Создает рёбра между узлами графа
        /// </summary>
        private void CreateGraphEdges(FuelStationGraph graph, FuelPlanningContext context)
        {
            var maxRange = context.TankCapacity / context.FuelConsumptionPerKm;

            foreach (var fromNode in graph.Nodes.Values)
            {
                foreach (var toNode in graph.Nodes.Values)
                {
                    if (fromNode.Id == toNode.Id) continue;
                    if (toNode.Position <= fromNode.Position) continue; // Только вперед

                    var distance = toNode.Position - fromNode.Position;
                    
                    // Проверяем достижимость
                    if (distance > maxRange) continue;

                    // Создаем ребро если переход возможен
                    var edge = CreateEdgeIfValid(fromNode, toNode, context);
                    if (edge != null)
                    {
                        graph.AddEdge(edge);
                    }
                }
            }
        }

        /// <summary>
        /// Создает ребро между узлами если переход валиден
        /// </summary>
        private FuelGraphEdge CreateEdgeIfValid(
            FuelGraphNode fromNode, 
            FuelGraphNode toNode, 
            FuelPlanningContext context)
        {
            var distance = toNode.Position - fromNode.Position;
            var fuelNeeded = distance * context.FuelConsumptionPerKm;
            
            // Определяем начальный уровень топлива
            double startFuel;
            if (fromNode.NodeType == GraphNodeType.Start)
            {
                startFuel = context.CurrentFuelLiters;
            }
            else if (fromNode.NodeType == GraphNodeType.Station)
            {
                // Предполагаем прибытие с минимальным запасом и дозаправку
                var arrivalFuel = Math.Max(0, context.TankCapacity * 0.2); // 20% запас
                startFuel = arrivalFuel + (context.TankCapacity * fromNode.RefillLevel);
                startFuel = Math.Min(startFuel, context.TankCapacity);
            }
            else
            {
                return null; // Из финиша никуда не едем
            }

            // Проверяем, хватит ли топлива
            var fuelAtArrival = startFuel - fuelNeeded;
            var minReserve = context.TankCapacity * FuelPlanningConfig.MinReserveFactor;
            
            if (fuelAtArrival < 0) return null; // Не дойдем
            
            // 🎯 КРИТИЧЕСКАЯ ПРОВЕРКА: Для финиша проверяем требуемый остаток топлива
            if (toNode.NodeType == GraphNodeType.Finish)
            {
                if (fuelAtArrival < context.FinishFuel)
                {
                    // Недостаточно топлива для финиша с требуемым остатком
                    return null;
                }
                
                // ✅ Переход к финишу валиден - топлива хватает
                return new FuelGraphEdge
                {
                    FromNode = fromNode,
                    ToNode = toNode,
                    Distance = distance,
                    FuelUsed = fuelNeeded,
                    TransitionCost = 0, // Переход к финишу бесплатный
                    FuelAtDestination = fuelAtArrival
                };
            }
            
            // Для станций проверяем минимальный запас (кроме критических ситуаций)
            if (toNode.NodeType == GraphNodeType.Station && fuelAtArrival < minReserve)
            {
                // Разрешаем только если это критическая ситуация (мало топлива вначале)
                var fuelPercentage = (context.CurrentFuelLiters / context.TankCapacity);
                if (fromNode.NodeType != GraphNodeType.Start || fuelPercentage > 0.3)
                {
                    return null;
                }
            }

            // Рассчитываем стоимость перехода
            var transitionCost = CalculateTransitionCost(fromNode, toNode, fuelAtArrival, context);

            return new FuelGraphEdge
            {
                FromNode = fromNode,
                ToNode = toNode,
                Distance = distance,
                FuelUsed = fuelNeeded,
                TransitionCost = transitionCost,
                FuelAtDestination = fuelAtArrival
            };
        }

        /// <summary>
        /// Рассчитывает стоимость перехода между узлами
        /// </summary>
        private double CalculateTransitionCost(
            FuelGraphNode fromNode, 
            FuelGraphNode toNode, 
            double fuelAtArrival,
            FuelPlanningContext context)
        {
            if (toNode.NodeType == GraphNodeType.Finish)
            {
                // Переход к финишу - только расход топлива, без дозаправки
                return 0;
            }

            if (toNode.NodeType == GraphNodeType.Station)
            {
                // Переход к станции - стоимость дозаправки
                var refillAmount = CalculateOptimalRefillForStation(toNode, fuelAtArrival, context);
                var fuelCost = refillAmount * toNode.StationInfo.PricePerLiter;
                
                // Добавляем штраф за остановку для минимизации количества остановок
                var stopPenalty = 10.0; // Фиксированный штраф за остановку
                
                return fuelCost + stopPenalty;
            }

            return 0;
        }

        /// <summary>
        /// 🎯 Рассчитывает оптимальную дозаправку с учетом финишного топлива
        /// </summary>
        private double CalculateOptimalRefillForStation(
            FuelGraphNode stationNode, 
            double fuelAtArrival, 
            FuelPlanningContext context)
        {
            var distanceToFinish = context.TotalDistanceKm - stationNode.Position;
            var fuelNeededToFinish = distanceToFinish * context.FuelConsumptionPerKm;
            var totalFuelNeededForFinish = fuelNeededToFinish + context.FinishFuel;
            
            // Вариант 1: Минимальная заправка для достижения финиша
            var minRefillForFinish = Math.Max(0, totalFuelNeededForFinish - fuelAtArrival);
            
            // Вариант 2: Заправка согласно уровню узла
            var nodeRefillAmount = context.TankCapacity * stationNode.RefillLevel;
            
            // Вариант 3: Максимально возможная заправка
            var maxPossibleRefill = context.TankCapacity - fuelAtArrival;
            
            // Выбираем оптимальную стратегию:
            // Если можем дойти до финиша с текущим топливом - заправляемся согласно узлу
            if (fuelAtArrival >= totalFuelNeededForFinish)
            {
                return Math.Min(nodeRefillAmount, maxPossibleRefill);
            }
            else
            {
                // Иначе заправляемся минимум для достижения финиша, 
                // но не меньше чем предполагает узел (если это возможно)
                var optimalRefill = Math.Max(minRefillForFinish, 
                    Math.Min(nodeRefillAmount, maxPossibleRefill));
                return Math.Min(optimalRefill, maxPossibleRefill);
            }
        }

        /// <summary>
        /// Запускает алгоритм Дейкстры для поиска оптимального пути
        /// </summary>
        private OptimalPath RunDijkstraAlgorithm(FuelStationGraph graph, FuelPlanningContext context)
        {
            var distances = new Dictionary<string, double>();
            var previous = new Dictionary<string, string>();
            var visited = new HashSet<string>();
            var priorityQueue = new SortedSet<(double Distance, string NodeId)>();

            var startNodeId = "START";
            var finishNodeId = "FINISH";

            // Инициализация
            foreach (var node in graph.Nodes)
            {
                distances[node.Key] = double.MaxValue;
                previous[node.Key] = null;
            }

            distances[startNodeId] = 0;
            priorityQueue.Add((0, startNodeId));

            Console.WriteLine($"🔍 Запуск алгоритма Дейкстры для {graph.Nodes.Count} узлов...");

            // Основной цикл Дейкстры
            while (priorityQueue.Count > 0)
            {
                var (currentDistance, currentNodeId) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (visited.Contains(currentNodeId)) continue;
                visited.Add(currentNodeId);

                // Если достигли финиша, можем прекратить поиск
                if (currentNodeId == finishNodeId)
                {
                    Console.WriteLine($"✅ Найден путь до финиша с стоимостью: ${currentDistance:F2}");
                    break;
                }

                // Обрабатываем всех соседей
                var edges = graph.GetEdgesFrom(currentNodeId);
                foreach (var edge in edges)
                {
                    var neighborId = edge.ToNode.Id;
                    if (visited.Contains(neighborId)) continue;

                    var alternativeDistance = currentDistance + edge.TransitionCost;

                    if (alternativeDistance < distances[neighborId])
                    {
                        distances[neighborId] = alternativeDistance;
                        previous[neighborId] = currentNodeId;
                        priorityQueue.Add((alternativeDistance, neighborId));
                    }
                }
            }

            // Восстанавливаем путь
            return ReconstructPath(previous, distances, graph, startNodeId, finishNodeId);
        }

        /// <summary>
        /// Восстанавливает оптимальный путь из результатов Дейкстры
        /// </summary>
        private OptimalPath ReconstructPath(
            Dictionary<string, string> previous,
            Dictionary<string, double> distances,
            FuelStationGraph graph,
            string startNodeId,
            string finishNodeId)
        {
            if (!distances.ContainsKey(finishNodeId) || distances[finishNodeId] == double.MaxValue)
            {
                return new OptimalPath { IsValid = false };
            }

            var path = new List<FuelGraphNode>();
            var currentNodeId = finishNodeId;

            // Восстанавливаем путь от финиша к старту
            while (currentNodeId != null)
            {
                path.Add(graph.Nodes[currentNodeId]);
                currentNodeId = previous[currentNodeId];
            }

            path.Reverse(); // Разворачиваем, чтобы получить путь от старта к финишу

            // Фильтруем только станции (исключаем START и FINISH)
            var stationNodes = path
                .Where(node => node.NodeType == GraphNodeType.Station)
                .ToList();

            return new OptimalPath
            {
                IsValid = true,
                Stations = stationNodes.Select(node => node.StationInfo).ToList(),
                TotalCost = distances[finishNodeId],
                NodesInPath = path
            };
        }

        /// <summary>
        /// Преобразует найденный путь в результат
        /// </summary>
        private StopPlanInfo ConvertPathToStopPlanInfo(OptimalPath optimalPath, FuelPlanningContext context)
        {
            var stops = new List<FuelStopPlan>();
            var currentFuel = context.CurrentFuelLiters;
            var currentPosition = 0.0;

            foreach (var stationNode in optimalPath.NodesInPath.Where(n => n.NodeType == GraphNodeType.Station))
            {
                var station = stationNode.StationInfo;
                
                // Рассчитываем расход топлива до станции
                var distance = station.ForwardDistanceKm - currentPosition;
                var fuelUsed = distance * context.FuelConsumptionPerKm;
                var fuelAtArrival = currentFuel - fuelUsed;

                // 🎯 Используем улучшенную логику дозаправки с учетом финишного топлива
                var refillAmount = CalculateOptimalRefillForStation(stationNode, fuelAtArrival, context);

                var stop = new FuelStopPlan
                {
                    Station = station.Station,
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

            // 🔍 Проверим, достигнем ли финиша с требуемым топливом
            var finalDistance = context.TotalDistanceKm - currentPosition;
            var finalFuelUsed = finalDistance * context.FuelConsumptionPerKm;
            var fuelAtFinish = currentFuel - finalFuelUsed;

            Console.WriteLine($"🏁 Финишная проверка: Топливо на финише: {fuelAtFinish:F1}л, Требуется: {context.FinishFuel:F1}л");

            return new StopPlanInfo
            {
                StopPlan = stops,
                Finish = CreateFinishInfo(context, fuelAtFinish)
            };
        }

        /// <summary>
        /// 🎯 Проверяет возможность прямого маршрута до финиша без заправок
        /// </summary>
        private DirectRouteResult CheckDirectRouteToFinish(FuelPlanningContext context)
        {
            var totalFuelNeeded = context.TotalDistanceKm * context.FuelConsumptionPerKm;
            var fuelAtFinish = context.CurrentFuelLiters - totalFuelNeeded;
            
            var isPossible = fuelAtFinish >= context.FinishFuel;
            
            Console.WriteLine($"🧮 Прямой маршрут: Текущее топливо: {context.CurrentFuelLiters:F1}л, " +
                            $"Нужно для поездки: {totalFuelNeeded:F1}л, " +
                            $"Останется: {fuelAtFinish:F1}л, " +
                            $"Требуется на финише: {context.FinishFuel:F1}л");
            
            return new DirectRouteResult
            {
                IsPossible = isPossible,
                FuelAtFinish = Math.Max(0, fuelAtFinish)
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

        private FinishInfo CreateFinishInfo(FuelPlanningContext context, double? actualFuelAtFinish = null)
        {
            return new FinishInfo
            {
                RemainingFuelLiters = actualFuelAtFinish ?? context.FinishFuel
            };
        }

        #endregion
    }

    #region Graph Data Structures

    /// <summary>
    /// Граф заправочных станций
    /// </summary>
    public class FuelStationGraph
    {
        public Dictionary<string, FuelGraphNode> Nodes { get; } = new Dictionary<string, FuelGraphNode>();
        public List<FuelGraphEdge> Edges { get; } = new List<FuelGraphEdge>();

        public void AddNode(FuelGraphNode node)
        {
            Nodes[node.Id] = node;
        }

        public void AddEdge(FuelGraphEdge edge)
        {
            Edges.Add(edge);
        }

        public List<FuelGraphEdge> GetEdgesFrom(string nodeId)
        {
            return Edges.Where(e => e.FromNode.Id == nodeId).ToList();
        }
    }

    /// <summary>
    /// Узел графа (станция или специальная точка)
    /// </summary>
    public class FuelGraphNode
    {
        public string Id { get; set; }
        public GraphNodeType NodeType { get; set; }
        public double Position { get; set; } // Позиция на маршруте в км
        public double FuelLevel { get; set; } // Уровень топлива в узле
        public StationInfo StationInfo { get; set; }
        public double RefillLevel { get; set; } // Процент дозаправки (0.0 - 1.0)
    }

    /// <summary>
    /// Ребро графа (переход между узлами)
    /// </summary>
    public class FuelGraphEdge
    {
        public FuelGraphNode FromNode { get; set; }
        public FuelGraphNode ToNode { get; set; }
        public double Distance { get; set; }
        public double FuelUsed { get; set; }
        public double TransitionCost { get; set; } // Стоимость перехода (топливо + штрафы)
        public double FuelAtDestination { get; set; }
    }

    /// <summary>
    /// Тип узла в графе
    /// </summary>
    public enum GraphNodeType
    {
        Start,   // Начальная точка
        Station, // Заправочная станция
        Finish   // Финишная точка
    }

    /// <summary>
    /// Результат оптимального пути
    /// </summary>
    public class OptimalPath
    {
        public bool IsValid { get; set; }
        public List<StationInfo> Stations { get; set; } = new List<StationInfo>();
        public double TotalCost { get; set; }
        public List<FuelGraphNode> NodesInPath { get; set; } = new List<FuelGraphNode>();
    }

    /// <summary>
    /// Результат проверки прямого маршрута
    /// </summary>
    public class DirectRouteResult
    {
        public bool IsPossible { get; set; }
        public double FuelAtFinish { get; set; }
    }

    #endregion
}

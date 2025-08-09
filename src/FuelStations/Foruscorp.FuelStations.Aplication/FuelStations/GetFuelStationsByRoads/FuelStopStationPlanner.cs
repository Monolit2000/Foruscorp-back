using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class FuelStopStationPlanner
    {
        private const double TankRestrictions = 40.0;
        private const double SearchRadiusKm = 9.0;

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
            // Validate finishFuel
            if (finishFuel < 0 || finishFuel > tankCapacity)
                throw new ArgumentException($"finishLiters must be between 0 and tankCapacity ({tankCapacity} liters).");

            // 1) Подготовка списка StationInfo (с километражем и ценой)
            var stationInfos = new List<StationInfo>();
            foreach (var st in stationsAlongRoute)
            {
                double forwardDist = GetForwardDistanceAlongRoute(route, st.Coordinates);
                if (forwardDist < double.MaxValue)
                {
                    var cheapestFuelPrice = st.FuelPrices
                        .Where(fp => fp.PriceAfterDiscount >= 0)
                        .OrderBy(fp => fp.PriceAfterDiscount)
                        .FirstOrDefault();

                    double pricePerLiter = cheapestFuelPrice?.PriceAfterDiscount ?? double.MaxValue;

                    stationInfos.Add(new StationInfo
                    {
                        Station = st,
                        ForwardDistanceKm = forwardDist,
                        PricePerLiter = pricePerLiter
                    });
                }
            }

            var endInfo = new StationInfo
            {
                Station = null,
                ForwardDistanceKm = totalRouteDistanceKm,
                PricePerLiter = 0.0
            };
            stationInfos.Add(endInfo);

            // 2) Собираем обязательные остановки и сортируем по километражу
            var requiredInfos = requiredStops
                .Select(r =>
                {
                    var si = stationInfos.FirstOrDefault(x => x.Station != null && x.Station.Id == r.StationId);
                    if (si == null) return null;
                    return new
                    {
                        Info = si,
                        RefillLiters = Math.Min(r.RefillLiters, tankCapacity)
                    };
                })
                .Where(x => x != null)
                .OrderBy(x => x.Info.ForwardDistanceKm)
                .ToList();

            var result = new List<FuelStopPlan>();
            var usedStationIds = new HashSet<Guid>();
            double prevKm = 0.0;
            double remainingFuel = currentFuelLiters;

            bool isFirstStop = true;

            // Вспомогательный метод: планирует промежуточные дозаправки, пока не достигнем targetKm
            void PlanTill(double targetKm, bool useMinDistance = false, double minStopDistanceKm = 1200.0)
            {
                // Сколько топлива надо, чтобы доехать от prevKm до targetKm, с учётом finishFuel для финального сегмента
                double neededFuel = (targetKm - prevKm) * fuelConsumptionPerKm + (targetKm == totalRouteDistanceKm ? finishFuel : 0);

                double extraRange = (tankCapacity - TankRestrictions) / fuelConsumptionPerKm;

                while (remainingFuel < neededFuel)
                {
                    double maxDistanceWithoutRefuel = remainingFuel / fuelConsumptionPerKm;
                    double maxReachKm = prevKm + maxDistanceWithoutRefuel;

                    // Первая попытка: с учётом minStopDistanceKm
                    var candidates = stationInfos
                        .Where(si =>
                            si.Station != null &&
                            !usedStationIds.Contains(si.Station.Id) &&
                            si.ForwardDistanceKm > prevKm &&
                            (
                                // экстренно, если до minStopDistance топлива не хватает,
                                // или станция удовлетворяет минимальную дистанцию
                                maxDistanceWithoutRefuel < minStopDistanceKm ||
                                si.ForwardDistanceKm - prevKm >= minStopDistanceKm
                            ) &&
                            si.ForwardDistanceKm <= maxReachKm
                        )
                        .OrderBy(si => si.PricePerLiter)
                        .ToList();

                    if (!candidates.Any() && useMinDistance)
                    {
                        candidates = stationInfos
                            .Where(si =>
                                si.Station != null &&
                                !usedStationIds.Contains(si.Station.Id) &&
                                si.ForwardDistanceKm > prevKm &&
                                si.ForwardDistanceKm <= maxReachKm
                            )
                            .OrderBy(si => si.PricePerLiter)
                            .ToList();
                    }

                    if (!candidates.Any())
                    {
                        break;
                    }

                    var best = candidates.First();

                    // Доезжаем до best
                    double dist = best.ForwardDistanceKm - prevKm;
                    remainingFuel -= dist * fuelConsumptionPerKm;

                    double preRemainingFuel = remainingFuel;

                    bool isLastRefuel = best.ForwardDistanceKm + extraRange >= totalRouteDistanceKm;

                    double effectiveCapacity = (isFirstStop || isLastRefuel)
                       ? tankCapacity + TankRestrictions
                       : tankCapacity;

                    // Дозаправка: для последнего сегмента учитываем finishFuel
                    double freeSpace = effectiveCapacity - remainingFuel;
                    double rawRefill;

                    if (targetKm == totalRouteDistanceKm && (remainingFuel >= neededFuel))
                    {
                        // Для финального сегмента рассчитываем, чтобы осталось ровно finishFuel
                        double fuelToTarget = (targetKm - best.ForwardDistanceKm) * fuelConsumptionPerKm;
                        rawRefill = fuelToTarget + finishFuel - remainingFuel;
                    }
                    else
                    {
                        // Для промежуточных остановок заливаем кратно 5 литрам
                        rawRefill = freeSpace;
                    }

                    double refill = Math.Floor(rawRefill / 5.0) * 5.0;

                    if (refill == 0 && freeSpace >= 5.0)
                        refill = 5.0;
                    else if (refill == 0 && freeSpace < 5.0)
                        refill = freeSpace; // мелкий остаток, зальём его

                    // Проверяем, что дозаправка не превышает доступное место в баке
                    refill = Math.Min(refill, freeSpace);

                    if (refill < 0)
                    {
                        refill = freeSpace; // Fallback to fill tank
                    }

                    remainingFuel += refill;

                    result.Add(new FuelStopPlan
                    {
                        Station = best.Station!,
                        StopAtKm = best.ForwardDistanceKm,
                        RefillLiters = refill,
                        CurrentFuelLiters = preRemainingFuel,
                        RoadSectionId = roadSectionId ?? string.Empty
                    });
                    usedStationIds.Add(best.Station!.Id);
                    prevKm = best.ForwardDistanceKm;

                    if (isFirstStop)
                        isFirstStop = false;

                    // Пересчитаем, сколько ещё топлива нужно
                    neededFuel = (targetKm - prevKm) * fuelConsumptionPerKm + (targetKm == totalRouteDistanceKm ? finishFuel : 0);
                }

                // «Проезжаем» до targetKm
                remainingFuel -= (targetKm - prevKm) * fuelConsumptionPerKm;
                prevKm = targetKm;
            }

            // 3) Для каждого обязательного сегмента: сначала промежуточные, затем обязательная
            foreach (var req in requiredInfos)
            {
                double kmReq = req.Info.ForwardDistanceKm;

                // Дозаправки между prevKm и обязательной (игнорируем MinStopDistance)
                PlanTill(kmReq);

                // Теперь делаем саму обязательную дозаправку ровно req.RefillLiters
                double allowed = Math.Min(req.RefillLiters, tankCapacity - remainingFuel);

                double preRemainingFuel = remainingFuel;

                remainingFuel += allowed;
                result.Add(new FuelStopPlan
                {
                    Station = req.Info.Station!,
                    StopAtKm = kmReq,
                    RefillLiters = allowed,
                    CurrentFuelLiters = preRemainingFuel,
                    RoadSectionId = roadSectionId ?? string.Empty
                });
                usedStationIds.Add(req.Info.Station!.Id);
            }

            // 4) Наконец, от последней обязательной до конца маршрута 
            PlanTill(totalRouteDistanceKm, useMinDistance: true);

            // 5) Корректируем последнюю заправку, если remainingFuel != finishFuel
            if (Math.Abs(remainingFuel - finishFuel) > 0.0001 && result.Any())
            {
                // Берем последнюю заправку
                var lastStop = result.Last();
                double fuelToFinish = (totalRouteDistanceKm - lastStop.StopAtKm) * fuelConsumptionPerKm;
                double requiredRefill = fuelToFinish + finishFuel - lastStop.CurrentFuelLiters;
                requiredRefill = Math.Min(requiredRefill, tankCapacity + 40 - lastStop.CurrentFuelLiters);

                if (requiredRefill >= 0)
                {
                    lastStop.RefillLiters = requiredRefill;
                    remainingFuel = lastStop.CurrentFuelLiters + requiredRefill - fuelToFinish;
                }
            }

            // 6) Формируем объект финиша
            var finishInfo = new FinishInfo
            {
                RemainingFuelLiters = remainingFuel,
            };

            return new StopPlanInfo { StopPlan = result, Finish = finishInfo };
        }

        private double GetForwardDistanceAlongRoute(List<GeoPoint> route, GeoPoint stationCoords)
        {
            double cumulative = 0.0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                var a = route[i];
                var b = route[i + 1];
                double segmentLength = GeoCalculator.CalculateHaversineDistance(a, b);
                double distToSegment = GeoCalculator.DistanceFromPointToSegmentKm(stationCoords, a, b);
                if (distToSegment <= SearchRadiusKm)
                {
                    // Считаем проекцию станции на этот отрезок
                    double projectionKm = GeoCalculator.DistanceAlongSegment(a, b, stationCoords);
                    return cumulative + projectionKm;
                }
                cumulative += segmentLength;
            }
            return double.MaxValue; // Станция «не попала» ни в один сегмент коридора
        }

        private class StationInfo
        {
            public FuelStation? Station { get; set; }
            public double ForwardDistanceKm { get; set; }
            public double PricePerLiter { get; set; }
        }
    }
}

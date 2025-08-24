using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadLovesStores
{
    public record LoadLovesStoresCommand : IRequest<Result>;
    
    public class LoadLovesStoresCommandHandler : IRequestHandler<LoadLovesStoresCommand, Result>
    {
        private readonly ILovesApiService _lovesApiService;
        private readonly IFuelStationContext _fuelStationContext;
        private readonly ILogger<LoadLovesStoresCommandHandler> _logger;

        public LoadLovesStoresCommandHandler(
            ILovesApiService lovesApiService,
            IFuelStationContext fuelStationContext,
            ILogger<LoadLovesStoresCommandHandler> logger)
        {
            _lovesApiService = lovesApiService;
            _fuelStationContext = fuelStationContext;
            _logger = logger;
        }

        public async Task<Result> Handle(LoadLovesStoresCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting to load Love's stores from API");

                var apiResponse = await _lovesApiService.GetStoresAsync(cancellationToken);

                if (apiResponse == null || apiResponse.Stores == null || !apiResponse.Stores.Any())
                {
                    _logger.LogWarning("No stores received from Love's API");
                    return Result.Fail("No stores received from Love's API");
                }

                _logger.LogInformation("Successfully loaded {StoreCount} Love's stores from API", 
                    apiResponse.Stores.Count);

                // Загружаем существующие станции из базы данных
                var existingFuelStations = await _fuelStationContext.FuelStations.ToListAsync(cancellationToken);

                var newStationsList = new List<FuelStation>();

                foreach (var store in apiResponse.Stores)
                {
                    // Ищем существующую станцию по координатам (с округлением до 3 знаков)
                    var existingStation = existingFuelStations.FirstOrDefault(s =>
                        (Math.Round(s.Coordinates.Longitude, 3) == Math.Round(store.Longitude, 3) && 
                        Math.Round(s.Coordinates.Latitude, 3) == Math.Round(store.Latitude, 3)) || 
                        (s.FuelStationProviderId != null && s.FuelStationProviderId == store.Name));


                    if (existingStation is not null && (existingStation.SystemFuelProvider == SystemProvider.Loves || existingStation.SystemFuelProvider == SystemProvider.Unknown))
                    {
                        // Обновляем существующую станцию

                        if( string.IsNullOrEmpty(existingStation.FuelStationProviderId))
                            existingStation.FuelStationProviderId = store.Number.ToString();

                        existingStation.ProviderName = SystemProvider.Loves.ToString();
                        
                        // Обновляем адрес, если он изменился
                        var fullAddress = $"{store.Address1} {store.City}, {store.State} {store.Zip}".Trim();
                        if (existingStation.Address != fullAddress)
                        {
                            existingStation.UpdateLocation(fullAddress, new GeoPoint(store.Latitude, store.Longitude));
                        }
                        existingStation.SystemFuelProvider = SystemProvider.Loves;

                        existingStation.LastUpdated = DateTime.UtcNow;

                        _fuelStationContext.FuelStations.Update(existingStation);
                    }
                    else
                    {
                        // Создаем новую станцию
                        var fullAddress = $"{store.Address1} {store.City}, {store.State} {store.Zip}".Trim();
                        
                        var newFuelStation = FuelStation.CreateNew(
                            fullAddress,
                            SystemProvider.Loves.ToString(),
                            new GeoPoint(store.Latitude, store.Longitude)
                        );

                        newFuelStation.FuelStationProviderId = store.Number.ToString();
                        //newFuelStation.ProviderName = "Love's";

                        newFuelStation.SystemFuelProvider = SystemProvider.Loves;

                        newStationsList.Add(newFuelStation);
                    }
                }

                // Добавляем новые станции в базу данных
                if (newStationsList.Any())
                {
                    await _fuelStationContext.FuelStations.AddRangeAsync(newStationsList, cancellationToken);
                    _logger.LogInformation("Adding {NewStationCount} new Love's stations to database", newStationsList.Count);
                }

                // Сохраняем изменения
                await _fuelStationContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully processed Love's stores. Added {NewCount} new stations, updated existing stations", 
                    newStationsList.Count);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing Love's stores");
                return Result.Fail($"Error processing Love's stores: {ex.Message}");
            }
        }
    }
}

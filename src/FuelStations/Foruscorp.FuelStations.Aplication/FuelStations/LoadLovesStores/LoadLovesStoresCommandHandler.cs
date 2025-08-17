using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadLovesStores
{
    public record LoadLovesStoresCommand : IRequest<Result<LovesApiResponseModel>>;
    
    public class LoadLovesStoresCommandHandler : IRequestHandler<LoadLovesStoresCommand, Result<LovesApiResponseModel>>
    {
        private readonly ILovesApiService _lovesApiService;
        private readonly ILogger<LoadLovesStoresCommandHandler> _logger;

        public LoadLovesStoresCommandHandler(
            ILovesApiService lovesApiService,
            ILogger<LoadLovesStoresCommandHandler> logger)
        {
            _lovesApiService = lovesApiService;
            _logger = logger;
        }

        public async Task<Result<LovesApiResponseModel>> Handle(LoadLovesStoresCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting to load Love's stores from API");

                var result = await _lovesApiService.GetStoresAsync(cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Failed to load Love's stores from API");
                    return Result.Fail<LovesApiResponseModel>("Failed to load Love's stores from API");
                }

                _logger.LogInformation("Successfully loaded {StoreCount} Love's stores from API", 
                    result.Stores?.Count ?? 0);

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while loading Love's stores");
                return Result.Fail<LovesApiResponseModel>($"Error loading Love's stores: {ex.Message}");
            }
        }
    }
}

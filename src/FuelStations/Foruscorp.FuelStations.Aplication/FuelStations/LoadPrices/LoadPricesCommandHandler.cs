using FluentResults;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadLoversPrice;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadTaAndPetroPrice;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadPrices
{
    public record LoadPricesCommand(List<IFormFile> files) : IRequest<Result>;
    
    public class LoadPricesCommandHandler : IRequestHandler<LoadPricesCommand, Result>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LoadPricesCommandHandler> _logger;

        public LoadPricesCommandHandler(
            IMediator mediator,
            ILogger<LoadPricesCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result> Handle(LoadPricesCommand request, CancellationToken cancellationToken)
        {
            if (request.files == null || !request.files.Any())
            {
                return Result.Fail("No files provided.");
            }

            var results = new List<Result>();
            var processedFiles = new List<string>();

            foreach (var file in request.files)
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Skipping empty file: {FileName}", file?.FileName ?? "null");
                    continue;
                }

                try
                {
                    var fileType = DetermineFileType(file.FileName);
                    Result result;

                    switch (fileType)
                    {
                        case FileType.Lovers:
                            _logger.LogInformation("Processing Love's price file: {FileName}", file.FileName);
                            var loversCommand = new LoadLoversPriceCommand(file);
                            result = await _mediator.Send(loversCommand, cancellationToken);
                            break;

                        case FileType.TaAndPetro:
                            _logger.LogInformation("Processing TA & Petro price file: {FileName}", file.FileName);
                            var taAndPetroCommand = new LoadTaAndPetroPriceCommand(file);
                            result = await _mediator.Send(taAndPetroCommand, cancellationToken);
                            break;

                        default:
                            _logger.LogWarning("Unknown file type for file: {FileName}", file.FileName);
                            result = Result.Fail($"Unknown file type for file: {file.FileName}");
                            break;
                    }

                    results.Add(result);
                    processedFiles.Add(file.FileName);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Successfully processed file: {FileName}", file.FileName);
                    }
                    else
                    {
                        _logger.LogError("Failed to process file: {FileName}. Errors: {Errors}", 
                            file.FileName, string.Join(", ", result.Errors));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing file: {FileName}", file.FileName);
                    results.Add(Result.Fail($"Error processing file {file.FileName}: {ex.Message}"));
                    processedFiles.Add(file.FileName);
                }
            }

            // Проверяем результаты
            var failedResults = results.Where(r => r.IsFailed).ToList();
            var successfulResults = results.Where(r => r.IsSuccess).ToList();

            _logger.LogInformation("Processing completed. Successfully processed: {SuccessCount}, Failed: {FailedCount}", 
                successfulResults.Count, failedResults.Count);

            if (failedResults.Any())
            {
                var errorMessages = failedResults
                    .SelectMany(r => r.Errors)
                    .ToList();

                return Result.Fail($"Some files failed to process. Successfully processed: {successfulResults.Count}, Failed: {failedResults.Count}. Errors: {string.Join("; ", errorMessages)}");
            }

            return Result.Ok();
        }

        private FileType DetermineFileType(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return FileType.Unknown;

            var lowerFileName = fileName.ToLowerInvariant();

            if (lowerFileName.Contains("loves") || 
                lowerFileName.Contains("lovers") || 
                lowerFileName.Contains("love's") || 
                lowerFileName.Contains("love") ||
                lowerFileName.Contains("loves_") ||
                lowerFileName.Contains("lovers_"))
            {
                return FileType.Lovers;
            }

            if (lowerFileName.Contains("ta") || 
                lowerFileName.Contains("petro") || 
                lowerFileName.Contains("ta&petro") ||
                lowerFileName.Contains("ta_and_petro") ||
                lowerFileName.Contains("taandpetro"))
            {
                return FileType.TaAndPetro;
            }

            // Дополнительная логика определения по расширению или другим признакам
            if (lowerFileName.EndsWith(".xlsx") || lowerFileName.EndsWith(".xls"))
            {
                // Если это Excel файл, но не можем определить тип, возвращаем Unknown
                return FileType.Unknown;
            }

            return FileType.Unknown;
        }
    }

    public enum FileType
    {
        Unknown,
        Lovers,
        TaAndPetro
    }
}

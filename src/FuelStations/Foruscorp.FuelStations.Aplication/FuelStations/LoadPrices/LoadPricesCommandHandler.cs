using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadLoversPrice;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadTaAndPetroPrice;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly IFuelStationContext _fuelStationContext;
        private readonly ILogger<LoadPricesCommandHandler> _logger;
        private readonly IExcelConverterService _excelConverterService;

        public LoadPricesCommandHandler(
            IMediator mediator,
            IFuelStationContext fuelStationContext,
            ILogger<LoadPricesCommandHandler> logger,
            IExcelConverterService excelConverterService)
        {
            _mediator = mediator;
            _fuelStationContext = fuelStationContext;
            _logger = logger;
            _excelConverterService = excelConverterService;
        }

        public async Task<Result> Handle(LoadPricesCommand request, CancellationToken cancellationToken)
        {
            if (request.files == null || !request.files.Any())
            {
                return Result.Fail("No files provided.");
            }

            // Создаем запись о попытке загрузки
            var priceLoadAttempt = PriceLoadAttempt.CreateNew(request.files.Count);
            await _fuelStationContext.PriceLoadAttempts.AddAsync(priceLoadAttempt, cancellationToken);
            await _fuelStationContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Started price load attempt {AttemptId} with {FileCount} files", 
                priceLoadAttempt.Id, request.files.Count);

            var results = new List<Result>();
            var processedFiles = new List<string>();

            foreach (var file in request.files)
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Skipping empty file: {FileName}", file?.FileName ?? "null");
                    priceLoadAttempt.AddFileResult(file?.FileName ?? "null", false, "File is empty or null");
                    continue;
                }

                try
                {
                    // Проверяем, нужно ли конвертировать XLS в XLSX
                    IFormFile fileToProcess = file;
                    if (_excelConverterService.IsXlsFile(file.FileName))
                    {
                        _logger.LogInformation("Converting XLS file to XLSX: {FileName}", file.FileName);
                        try
                        {
                            fileToProcess = await _excelConverterService.ConvertXlsToXlsxAsync(file, cancellationToken);
                            _logger.LogInformation("Successfully converted XLS to XLSX: {OriginalFileName} -> {ConvertedFileName}", 
                                file.FileName, fileToProcess.FileName);
                        }
                        catch (Exception conversionEx)
                        {
                            _logger.LogError(conversionEx, "Failed to convert XLS file to XLSX: {FileName}", file.FileName);
                            var conversionError = $"Failed to convert XLS file {file.FileName} to XLSX: {conversionEx.Message}";
                            results.Add(Result.Fail(conversionError));
                            processedFiles.Add(file.FileName);
                            priceLoadAttempt.AddFileResult(file.FileName, false, conversionError);
                            continue;
                        }
                    }

                    var fileType = DetermineFileType(fileToProcess.FileName);
                    Result result;

                    switch (fileType)
                    {
                        case FileType.Lovers:
                            _logger.LogInformation("Processing Love's price file: {FileName}", fileToProcess.FileName);
                            var loversCommand = new LoadLoversPriceCommand(fileToProcess);
                            result = await _mediator.Send(loversCommand, cancellationToken);
                            break;

                        case FileType.TaAndPetro:
                            _logger.LogInformation("Processing TA & Petro price file: {FileName}", fileToProcess.FileName);
                            var taAndPetroCommand = new LoadTaAndPetroPriceCommand(fileToProcess);
                            result = await _mediator.Send(taAndPetroCommand, cancellationToken);
                            break;

                        default:
                            _logger.LogWarning("Unknown file type for file: {FileName}", fileToProcess.FileName);
                            result = Result.Fail($"Unknown file type for file: {fileToProcess.FileName}");
                            break;
                    }

                    results.Add(result);
                    processedFiles.Add(file.FileName);

                    // Добавляем результат обработки файла
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Successfully processed file: {FileName}", file.FileName);
                        priceLoadAttempt.AddFileResult(file.FileName, true);
                    }
                    else
                    {
                        _logger.LogError("Failed to process file: {FileName}. Errors: {Errors}", 
                            file.FileName, string.Join(", ", result.Errors));
                        priceLoadAttempt.AddFileResult(file.FileName, false, string.Join("; ", result.Errors));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing file: {FileName}", file.FileName);
                    var errorMessage = $"Error processing file {file.FileName}: {ex.Message}";
                    results.Add(Result.Fail(errorMessage));
                    processedFiles.Add(file.FileName);
                    priceLoadAttempt.AddFileResult(file.FileName, false, errorMessage);
                }
            }

            // Проверяем результаты
            var failedResults = results.Where(r => r.IsFailed).ToList();
            var successfulResults = results.Where(r => r.IsSuccess).ToList();

            _logger.LogInformation("Processing completed. Successfully processed: {SuccessCount}, Failed: {FailedCount}", 
                successfulResults.Count, failedResults.Count);

            // Завершаем попытку загрузки
            bool isOverallSuccess = !failedResults.Any();
            string overallErrorMessage = null;

            if (failedResults.Any())
            {
                var errorMessages = failedResults
                    .SelectMany(r => r.Errors)
                    .ToList();
                overallErrorMessage = $"Some files failed to process. Successfully processed: {successfulResults.Count}, Failed: {failedResults.Count}. Errors: {string.Join("; ", errorMessages)}";
            }

            priceLoadAttempt.Complete(isOverallSuccess, overallErrorMessage);

            await _fuelStationContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Completed price load attempt {AttemptId}. Success rate: {SuccessRate}%, Duration: {Duration}", 
                priceLoadAttempt.Id, priceLoadAttempt.GetSuccessRate(), priceLoadAttempt.GetProcessingDuration());

            if (isOverallSuccess)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail(overallErrorMessage);
            }
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
                // Если это Excel файл, но не можем определить тип по имени, возвращаем Unknown
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

using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.FuelStations.LoadPrices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LoadPrices.GetPriceLoadAttempts
{
    public record GetPriceLoadAttemptsQuery(
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        bool? IsSuccessful = null,
        int? Page = null,
        int? PageSize = null) : IRequest<Result<List<PriceLoadAttemptDto>>>;

    public class GetPriceLoadAttemptsQueryHandler : IRequestHandler<GetPriceLoadAttemptsQuery, Result<List<PriceLoadAttemptDto>>>
    {
        private readonly IFuelStationContext _fuelStationContext;

        public GetPriceLoadAttemptsQueryHandler(IFuelStationContext fuelStationContext)
        {
            _fuelStationContext = fuelStationContext;
        }

        public async Task<Result<List<PriceLoadAttemptDto>>> Handle(GetPriceLoadAttemptsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _fuelStationContext.PriceLoadAttempts
                    .Include(pla => pla.FileResults)
                    .AsQueryable();

                // Применяем фильтры
                if (request.FromDate.HasValue)
                {
                    query = query.Where(pla => pla.StartedAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(pla => pla.StartedAt <= request.ToDate.Value);
                }

                if (request.IsSuccessful.HasValue)
                {
                    query = query.Where(pla => pla.IsSuccessful == request.IsSuccessful.Value);
                }

                // Сортируем по дате начала (новые сначала)
                query = query.OrderByDescending(pla => pla.StartedAt);

                // Применяем пагинацию
                if (request.Page.HasValue && request.PageSize.HasValue)
                {
                    var skip = (request.Page.Value - 1) * request.PageSize.Value;
                    query = query.Skip(skip).Take(request.PageSize.Value);
                }

                var attempts = await query.ToListAsync(cancellationToken);

                var dtos = attempts.Select(attempt => new PriceLoadAttemptDto
                {
                    Id = attempt.Id,
                    StartedAt = attempt.StartedAt,
                    CompletedAt = attempt.CompletedAt,
                    IsSuccessful = attempt.IsSuccessful,
                    ErrorMessage = attempt.ErrorMessage,
                    TotalFiles = attempt.TotalFiles,
                    SuccessfullyProcessedFiles = attempt.SuccessfullyProcessedFiles,
                    FailedFiles = attempt.FailedFiles,
                    SuccessRate = attempt.GetSuccessRate(),
                    ProcessingDuration = attempt.GetProcessingDuration(),
                    FileResults = attempt.FileResults.Select(fr => new FileProcessingResultDto
                    {
                        FileName = fr.FileName,
                        IsSuccess = fr.IsSuccess,
                        ErrorMessage = fr.ErrorMessage,
                        ProcessedAt = fr.ProcessedAt
                    }).ToList()
                }).ToList();

                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error retrieving price load attempts: {ex.Message}");
            }
        }
    }
}

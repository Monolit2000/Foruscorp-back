using FluentResults;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Reports.GetReportLoadAttempts
{
    public record GetReportLoadAttemptsQuery(
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        bool? IsSuccessful = null) : IRequest<Result<List<ReportLoadAttemptDto>>>;

    public class GetReportLoadAttemptsQueryHandler : IRequestHandler<GetReportLoadAttemptsQuery, Result<List<ReportLoadAttemptDto>>>
    {
        private readonly ITruckTrackingContext _truckTrackingContext;

        public GetReportLoadAttemptsQueryHandler(ITruckTrackingContext truckTrackingContext)
        {
            _truckTrackingContext = truckTrackingContext;
        }

        public async Task<Result<List<ReportLoadAttemptDto>>> Handle(GetReportLoadAttemptsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _truckTrackingContext.ReportLoadAttempts
                    .Include(rla => rla.FileResults)
                    .AsQueryable();

                // Применяем фильтры
                if (request.FromDate.HasValue)
                {
                    query = query.Where(rla => rla.StartedAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(rla => rla.StartedAt <= request.ToDate.Value);
                }

                if (request.IsSuccessful.HasValue)
                {
                    query = query.Where(rla => rla.IsSuccessful == request.IsSuccessful.Value);
                }

                // Сортируем по дате начала (новые сначала)
                query = query.OrderByDescending(rla => rla.StartedAt);

                var attempts = await query.ToListAsync(cancellationToken);

                var dtos = attempts.Select(attempt => new ReportLoadAttemptDto
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
                return Result.Fail($"Error retrieving report load attempts: {ex.Message}");
            }
        }
    }
}

using Foruscorp.Trucks.Aplication.Reports;
using Foruscorp.Trucks.Aplication.Transactions;
using Foruscorp.Trucks.Domain.Reports.GetReportLoadAttempts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Parse PDF transaction report and extract transaction data
        /// </summary>
        /// <param name="file">PDF file containing transaction report</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parsed transactions</returns>
        [HttpPost("load-report")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TransactionDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TransactionDto>>> ParsePdfTransactions(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (file == null)
                {
                    return BadRequest("PDF file is required");
                }

                var command = new ParsePdfTransactionsCommand(file);
                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Get report load attempts with optional filtering
        /// </summary>
        /// <param name="fromDate">Filter from date</param>
        /// <param name="toDate">Filter to date</param>
        /// <param name="isSuccessful">Filter by success status</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of report load attempts</returns>
        [HttpGet("report-load-attempts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ReportLoadAttemptDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ReportLoadAttemptDto>>> GetReportLoadAttempts(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? isSuccessful = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetReportLoadAttemptsQuery(fromDate, toDate, isSuccessful);
                var result = await _mediator.Send(query, cancellationToken);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

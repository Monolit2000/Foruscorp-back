using Microsoft.AspNetCore.Mvc;
using Foruscorp.TrucksTracking.Aplication.TransactionReports;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionReportController : ControllerBase
    {
        private readonly ITransactionReportService _transactionReportService;

        public TransactionReportController(ITransactionReportService transactionReportService)
        {
            _transactionReportService = transactionReportService;
        }

        [HttpPost("parse-pdf")]
        public async Task<ActionResult<List<TransactionReport>>> ParsePdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF files are allowed");
            }

            try
            {
                // Save the uploaded file temporarily
                var tempPath = Path.GetTempFileName();
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Parse the PDF
                var transactions = await _transactionReportService.ParsePdfAsync(tempPath);

                // Clean up the temporary file
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing PDF: {ex.Message}");
            }
        }

        [HttpPost("parse-and-save")]
        public async Task<ActionResult<List<TransactionReport>>> ParseAndSavePdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF files are allowed");
            }

            try
            {
                // Save the uploaded file temporarily
                var tempPath = Path.GetTempFileName();
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Parse the PDF
                var transactions = await _transactionReportService.ParsePdfAsync(tempPath);

                // Save each transaction to the database
                var savedTransactions = new List<TransactionReport>();
                foreach (var transaction in transactions)
                {
                    var savedTransaction = await _transactionReportService.SaveTransactionReportAsync(transaction);
                    savedTransactions.Add(savedTransaction);
                }

                // Clean up the temporary file
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }

                return Ok(savedTransactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing PDF: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<TransactionReport>>> GetAllTransactionReports()
        {
            try
            {
                var reports = await _transactionReportService.GetTransactionReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transaction reports: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionReport>> GetTransactionReportById(Guid id)
        {
            try
            {
                var report = await _transactionReportService.GetTransactionReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound();
                }
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transaction report: {ex.Message}");
            }
        }

        [HttpGet("card/{card}")]
        public async Task<ActionResult<List<TransactionReport>>> GetTransactionReportsByCard(string card)
        {
            try
            {
                var reports = await _transactionReportService.GetTransactionReportsByCardAsync(card);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transaction reports: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryTestController : ControllerBase
    {
        private readonly ILogger<TelemetryTestController> _logger;
        private static readonly ActivitySource ActivitySource = new("Trucks.API");

        public TelemetryTestController(ILogger<TelemetryTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Telemetry test endpoint called");
            
            using var activity = ActivitySource.StartActivity("TestOperation");
            activity?.SetTag("test.tag", "test-value");
            
            return Ok(new 
            { 
                message = "Telemetry test successful",
                timestamp = System.DateTime.UtcNow,
                traceId = Activity.Current?.TraceId.ToString(),
                spanId = Activity.Current?.SpanId.ToString()
            });
        }

        [HttpGet("log-test")]
        public IActionResult LogTest()
        {
            _logger.LogTrace("This is a trace log");
            _logger.LogDebug("This is a debug log");
            _logger.LogInformation("This is an information log");
            _logger.LogWarning("This is a warning log");
            _logger.LogError("This is an error log");
            _logger.LogCritical("This is a critical log");

            return Ok(new { message = "Log test completed" });
        }

        [HttpGet("exception-test")]
        public IActionResult ExceptionTest()
        {
            try
            {
                throw new InvalidOperationException("This is a test exception for telemetry");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test exception occurred");
                throw;
            }
        }
    }
}

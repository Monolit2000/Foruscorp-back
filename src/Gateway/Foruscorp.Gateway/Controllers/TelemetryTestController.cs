using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Foruscorp.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryTestController : ControllerBase
    {
        private readonly ILogger<TelemetryTestController> _logger;
        private static readonly ActivitySource ActivitySource = new("YarpGateway");

        public TelemetryTestController(ILogger<TelemetryTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Gateway telemetry test endpoint called");
            
            using var activity = ActivitySource.StartActivity("GatewayTestOperation");
            activity?.SetTag("gateway.test.tag", "gateway-test-value");
            
            return Ok(new 
            { 
                message = "Gateway telemetry test successful",
                timestamp = DateTime.UtcNow,
                traceId = Activity.Current?.TraceId.ToString(),
                spanId = Activity.Current?.SpanId.ToString(),
                service = "YarpGateway"
            });
        }

        [HttpGet("log-test")]
        public IActionResult LogTest()
        {
            _logger.LogTrace("Gateway trace log");
            _logger.LogDebug("Gateway debug log");
            _logger.LogInformation("Gateway information log");
            _logger.LogWarning("Gateway warning log");
            _logger.LogError("Gateway error log");
            _logger.LogCritical("Gateway critical log");

            return Ok(new { message = "Gateway log test completed" });
        }

        [HttpGet("headers")]
        public IActionResult Headers()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            
            _logger.LogInformation("Request headers: {@Headers}", headers);
            
            return Ok(new 
            { 
                headers = headers,
                traceParent = Request.Headers["traceparent"].FirstOrDefault(),
                traceState = Request.Headers["tracestate"].FirstOrDefault()
            });
        }
    }
}

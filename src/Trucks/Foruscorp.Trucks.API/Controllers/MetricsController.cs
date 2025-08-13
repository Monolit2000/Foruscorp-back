using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System;
using System.Diagnostics;
using System.Threading;

namespace Foruscorp.Trucks.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private static readonly Counter _requestCounter = Metrics.CreateCounter("trucks_api_requests_total", "Total number of requests", new CounterConfiguration
        {
            LabelNames = new[] { "endpoint", "method" }
        });

        private static readonly Histogram _requestDuration = Metrics.CreateHistogram("trucks_api_request_duration_seconds", "Request duration in seconds", new HistogramConfiguration
        {
            LabelNames = new[] { "endpoint" },
            Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
        });

        private static readonly Gauge _activeConnections = Metrics.CreateGauge("trucks_api_active_connections", "Number of active connections");

        private static readonly Counter _businessLogicCounter = Metrics.CreateCounter("trucks_api_business_operations_total", "Total number of business operations", new CounterConfiguration
        {
            LabelNames = new[] { "operation_type", "status" }
        });

        [HttpGet("test")]
        public IActionResult Test()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Увеличиваем счетчик запросов
                _requestCounter.WithLabels("test", "GET").Inc();
                
                // Симулируем бизнес-логику
                SimulateBusinessLogic();
                
                // Увеличиваем счетчик бизнес-операций
                _businessLogicCounter.WithLabels("test_operation", "success").Inc();
                
                stopwatch.Stop();
                
                // Записываем время выполнения
                _requestDuration.WithLabels("test").Observe(stopwatch.Elapsed.TotalSeconds);
                
                return Ok(new { message = "Test endpoint with metrics", duration = stopwatch.Elapsed.TotalSeconds });
            }
            catch (Exception)
            {
                _businessLogicCounter.WithLabels("test_operation", "error").Inc();
                throw;
            }
        }

        [HttpGet("connections")]
        public IActionResult GetConnections()
        {
            // Симулируем количество активных соединений
            var randomConnections = Random.Shared.Next(10, 100);
            _activeConnections.Set(randomConnections);
            
            return Ok(new { activeConnections = randomConnections });
        }

        [HttpPost("simulate-load")]
        public IActionResult SimulateLoad([FromBody] LoadSimulationRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _requestCounter.WithLabels("simulate_load", "POST").Inc();
                
                // Симулируем нагрузку
                Thread.Sleep(request.DelayMs);
                
                // Симулируем случайные ошибки
                if (Random.Shared.Next(100) < request.ErrorRate)
                {
                    _businessLogicCounter.WithLabels("load_simulation", "error").Inc();
                    return StatusCode(500, new { error = "Simulated error" });
                }
                
                _businessLogicCounter.WithLabels("load_simulation", "success").Inc();
                
                stopwatch.Stop();
                _requestDuration.WithLabels("simulate_load").Observe(stopwatch.Elapsed.TotalSeconds);
                
                return Ok(new { 
                    message = "Load simulation completed", 
                    duration = stopwatch.Elapsed.TotalSeconds,
                    delay = request.DelayMs,
                    errorRate = request.ErrorRate
                });
            }
            catch (Exception)
            {
                _businessLogicCounter.WithLabels("load_simulation", "error").Inc();
                throw;
            }
        }

        private void SimulateBusinessLogic()
        {
            // Симулируем работу бизнес-логики
            Thread.Sleep(Random.Shared.Next(50, 200));
        }
    }

    public class LoadSimulationRequest
    {
        public int DelayMs { get; set; } = 100;
        public int ErrorRate { get; set; } = 5; // процент ошибок
    }
}

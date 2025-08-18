using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.API.Middleware
{
    public class TraceContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TraceContextMiddleware> _logger;

        public TraceContextMiddleware(RequestDelegate next, ILogger<TraceContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceParent = context.Request.Headers["traceparent"].FirstOrDefault();
            var traceState = context.Request.Headers["tracestate"].FirstOrDefault();

            _logger.LogInformation("Processing request: {Method} {Path}, TraceParent: {TraceParent}", 
                context.Request.Method, context.Request.Path, traceParent);

            // Log additional trace information if available
            if (Activity.Current != null)
            {
                _logger.LogInformation("Current Activity: {ActivityId}, {TraceId}, {SpanId}", 
                    Activity.Current.Id, Activity.Current.TraceId, Activity.Current.SpanId);
            }

            await _next(context);

            _logger.LogInformation("Completed request: {Method} {Path}, StatusCode: {StatusCode}", 
                context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    }

    public static class TraceContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseTraceContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TraceContextMiddleware>();
        }
    }
}

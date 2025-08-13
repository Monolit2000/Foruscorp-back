using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add controllers for telemetry testing
builder.Services.AddControllers();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Prometheus metrics
builder.Services.AddHealthChecks();
builder.Services.AddMetricServer(options =>
{
    options.Port = 9090;
});

// Configure OpenTelemetry with full instrumentation
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("YarpGateway")
        .AddAttributes(new KeyValuePair<string, object>[]
        {
            new("service.instance.id", Environment.MachineName),
            new("service.version", "1.0.0")
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("YarpGateway"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            //options.EnableGrpcAspNetCoreSupport = true;
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequestMessage = (activity, request) =>
            {
                activity.SetTag("http.request.method", request.Method.ToString());
                activity.SetTag("http.request.url", request.RequestUri?.ToString());
            };
        })
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("YarpGateway"))
    .UseOtlpExporter();

// Configure logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("YarpGateway"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

var app = builder.Build();

// Middleware to ensure proper trace context propagation
app.Use(async (context, next) =>
{
    // Log incoming headers for debugging
    var traceParent = context.Request.Headers["traceparent"];
    var traceState = context.Request.Headers["tracestate"];
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Incoming request: {Method} {Path}, TraceParent: {TraceParent}, TraceState: {TraceState}", 
        context.Request.Method, context.Request.Path, traceParent, traceState);

    // Ensure trace context is properly propagated
    if (!string.IsNullOrEmpty(traceParent))
    {
        context.Request.Headers["traceparent"] = traceParent;
        if (!string.IsNullOrEmpty(traceState))
        {
            context.Request.Headers["tracestate"] = traceState;
        }
    }

    await next(context);
});

app.UseAuthentication();
app.UseAuthorization();

// Add Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

// Map controllers for telemetry testing
app.MapControllers();

app.MapReverseProxy();

app.Run();

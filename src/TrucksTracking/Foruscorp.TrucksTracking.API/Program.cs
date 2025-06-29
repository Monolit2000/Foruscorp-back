using Foruscorp.TrucksTracking.API.Realtime;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
using Foruscorp.TrucksTracking.Infrastructure.Percistence;
using Foruscorp.TrucksTracking.Infrastructure.Satup;
using MassTransit;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Create a custom meter for diagnostics
var meter = new Meter("TrucksTracking.Diagnostics");
var testCounter = meter.CreateCounter<long>("test_counter");

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("TrucksTracking.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)

        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://aspire-dashboard:18889");
            //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("TrucksTracking")
        .AddMeter("TrucksTracking.Diagnostics")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://aspire-dashboard:18889");
            //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }));

//// Configure logging
//builder.Logging.AddOpenTelemetry(logging => logging
//    .AddOtlpExporter(options =>
//    {
//        options.Endpoint = new Uri("http://aspire-dashboard:18889");
//        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
//    })).AddConsole();


builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration) // Read settings from appsettings.json
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console() // Keep console logging
        .WriteTo.Seq("http://seq:5341"); // Seq server URL (adjust as needed)
});


// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<TruckLocationUpdater>();
//builder.Services.AddSingleton<ActiveTruckManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddTrucksTrackingServices(builder.Configuration);

builder.Services.AddScoped<ISignalRNotificationSender, SignalRNotificationSender>();

builder.Services.AddSingleton<TruckGroupSubscriptionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyTuckTrackingMigrations();
}

app.UseCors("AllowAll");

app.MapHub<TruckHub>("/truck-tracking");


app.UseSerilogRequestLogging();

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
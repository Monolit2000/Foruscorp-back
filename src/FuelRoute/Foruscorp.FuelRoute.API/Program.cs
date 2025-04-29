using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Foruscorp.FuelRoutes.Infrastructure;
using Foruscorp.FuelStations.Infrastructure.Percistence;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System;
using Foruscorp.FuelStations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("FuelRoutes.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddSource("FuelRoutes")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://aspire-dashboard:18889");
            //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("FuelRoutes")
        .AddMeter("FuelRoutes.Diagnostics")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://aspire-dashboard:18889");
            //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }));

// Configure logging
builder.Logging.AddOpenTelemetry(logging => logging
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://aspire-dashboard:18889");
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    })).AddConsole();




builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


//builder.Services.AddHttpClient<IBasketService, BasketService>();

builder.Services.AddPersistenceServices(builder.Configuration); 


builder.Services.AddFuelStationServices(builder.Configuration); 




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyFuelRouteContextMigrations();
}

//app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();

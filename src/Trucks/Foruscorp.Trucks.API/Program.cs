using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.Trucks.Infrastructure.Satup;
using Foruscorp.Trucks.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("Trucks.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddSource("Trucks")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://aspire-dashboard:18889");
            //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Trucks")
        .AddMeter("Trucks.Diagnostics")
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


builder.Services.AddTrucksServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyTuckTrackingMigrations();
}

//app.UseAuthorization();

app.UseCors("AllowAll");


app.MapControllers();

app.Run();

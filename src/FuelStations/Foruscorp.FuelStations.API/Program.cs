using Foruscorp.FuelStations.Infrastructure;
using Foruscorp.FuelStations.Infrastructure.Percistence;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("FuelStations.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddSource()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://aspire-dashboard:18889");
            //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("FuelStations")
        .AddMeter("FuelStations.Diagnostics")
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



builder.Services.AddFuelStationServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyFuelStationContextMigrations();
}


app.UseCors("AllowAll");

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();

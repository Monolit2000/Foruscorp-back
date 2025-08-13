using ExpoCommunityNotificationServer.Client;
using Foruscorp.Push.Infrastructure;
using Foruscorp.Push.Infrastructure.Database;
using Scalar.AspNetCore;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Push.API"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddNpgsqlInstrumentation()
        .AddMeter("Push.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddNpgsql()
        .AddSource("Push.API"))
    .UseOtlpExporter();

// Configure logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("Push.API"));
});

builder.Services.AddPushInfrastructure(builder.Configuration);

builder.Services.AddSingleton<IPushApiClient>(_ =>
    new PushApiClient("ILMpwpYtrz4GAPh58NrxdNaYdGR5GLCMFOkaz2hz")
);

var app = builder.Build();

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1", permanent: false);
    return Task.CompletedTask;
});



//app.UsePathBase("/scalar/v1");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ApplyPushContextMigrations();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

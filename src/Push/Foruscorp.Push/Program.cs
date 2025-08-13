using ExpoCommunityNotificationServer.Client;
using Foruscorp.Push.Infrastructure;
using Foruscorp.Push.Infrastructure.Database;
using Scalar.AspNetCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Prometheus metrics
builder.Services.AddHealthChecks();
builder.Services.AddMetricServer(options =>
{
    options.Port = 9090;
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

// Add Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

app.MapControllers();

app.Run();

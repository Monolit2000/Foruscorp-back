using Foruscorp.TrucksTracking.Worker.Infrastructure.Database;
using Scalar.AspNetCore;
using Foruscorp.TrucksTracking.Worker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "foruscorp-redis";
    //options.InstanceName = "ForuscorpApp:";
});

builder.Services.AddTrucksTrackingWorkerServices(builder.Configuration);

var app = builder.Build();

app.MapScalarApiReference();


app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1", permanent: false);
    return Task.CompletedTask;
});
app.MapOpenApi();

app.ApplyTruckTrackerWorkerContextMigrations();

if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

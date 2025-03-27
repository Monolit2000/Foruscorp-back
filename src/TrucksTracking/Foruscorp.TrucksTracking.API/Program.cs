using Foruscorp.TrucksTracking.API.Realtime;
using Foruscorp.TrucksTracking.Infrastructure.Percistence;
using Foruscorp.TrucksTracking.Infrastructure.Satup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHostedService<TruckLocationUpdater>();
builder.Services.AddSingleton<ActiveTruckManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddTrucksTrackingServices(builder.Configuration);

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

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
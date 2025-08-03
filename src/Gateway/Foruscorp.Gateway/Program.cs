using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("YarpGateway"))
        .WithTracing(tracing =>
    tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .UseOtlpExporter();


//builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
//    .AddBearerToken();

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

//app.MapGet("login", () =>
//    Results.SignIn(
//        new ClaimsPrincipal(
//            new ClaimsIdentity(
//                new[]
//                {
//                    new Claim("sub", Guid.NewGuid().ToString())
//                },
//                BearerTokenDefaults.AuthenticationScheme)
//        ),
//        authenticationScheme: BearerTokenDefaults.AuthenticationScheme
//    )
//);

app.Use(async (context, next) =>
{
    // Log incoming headers for debugging
    var traceParent = context.Request.Headers["traceparent"];
    var traceState = context.Request.Headers["tracestate"];
    Console.WriteLine($"Incoming traceparent: {traceParent}, tracestate: {traceState}");

    await next(context);
});

app.UseAuthentication();

app.UseAuthorization();

app.MapReverseProxy();

//app.MapControllers();
app.Run();

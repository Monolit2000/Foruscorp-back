# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Trucks/Foruscorp.Trucks.API/Foruscorp.Trucks.API.csproj", "src/Trucks/Foruscorp.Trucks.API/"]
COPY ["src/Trucks/Foruscorp.Trucks.Aplication/Foruscorp.Trucks.Aplication.csproj", "src/Trucks/Foruscorp.Trucks.Aplication/"]
COPY ["src/TrucksTracking/Foruscorp.TrucksTracking.IntegrationEvents/Foruscorp.TrucksTracking.IntegrationEvents.csproj", "src/TrucksTracking/Foruscorp.TrucksTracking.IntegrationEvents/"]
COPY ["src/Trucks/Foruscorp.Trucks.Domain/Foruscorp.Trucks.Domain.csproj", "src/Trucks/Foruscorp.Trucks.Domain/"]
COPY ["src/BuildingBlocks/Foruscorp.BuildingBlocks.Domain/Foruscorp.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/Foruscorp.BuildingBlocks.Domain/"]
COPY ["src/Trucks/Foruscorp.Trucks.IntegrationEvents/Foruscorp.Trucks.IntegrationEvents.csproj", "src/Trucks/Foruscorp.Trucks.IntegrationEvents/"]
COPY ["src/Trucks/Foruscorp.Trucks.Infrastructure/Foruscorp.Trucks.Infrastructure.csproj", "src/Trucks/Foruscorp.Trucks.Infrastructure/"]
RUN dotnet restore "./src/Trucks/Foruscorp.Trucks.API/Foruscorp.Trucks.API.csproj"
COPY . .
WORKDIR "/src/src/Trucks/Foruscorp.Trucks.API"
RUN dotnet build "./Foruscorp.Trucks.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Foruscorp.Trucks.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Foruscorp.Trucks.API.dll"]
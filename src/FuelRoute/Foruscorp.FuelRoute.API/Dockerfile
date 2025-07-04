# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FuelRoute/Foruscorp.FuelRoute.API/Foruscorp.FuelRoutes.API.csproj", "src/FuelRoute/Foruscorp.FuelRoute.API/"]
COPY ["src/FuelRoute/Foruscorp.FuelRoute.Aplication/Foruscorp.FuelRoutes.Aplication.csproj", "src/FuelRoute/Foruscorp.FuelRoute.Aplication/"]
COPY ["src/BuildingBlocks/Foruscorp.BuildingBlocks.Infastructure/Foruscorp.BuildingBlocks.Infastructure.csproj", "src/BuildingBlocks/Foruscorp.BuildingBlocks.Infastructure/"]
COPY ["src/FuelStations/Foruscorp.FuelStations.Infrastructure/Foruscorp.FuelStations.Infrastructure.csproj", "src/FuelStations/Foruscorp.FuelStations.Infrastructure/"]
COPY ["src/FuelStations/Foruscorp.FuelStations.Aplication/Foruscorp.FuelStations.Aplication.csproj", "src/FuelStations/Foruscorp.FuelStations.Aplication/"]
COPY ["src/FuelStations/Foruscorp.FuelStations.Domain/Foruscorp.FuelStations.Domain.csproj", "src/FuelStations/Foruscorp.FuelStations.Domain/"]
COPY ["src/BuildingBlocks/Foruscorp.BuildingBlocks.Domain/Foruscorp.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/Foruscorp.BuildingBlocks.Domain/"]
COPY ["src/FuelRoute/Foruscorp.FuelRoute.Domain/Foruscorp.FuelRoutes.Domain.csproj", "src/FuelRoute/Foruscorp.FuelRoute.Domain/"]
COPY ["src/FuelRoute/Foruscorp.FuelRoutes.IntegrationEvents/Foruscorp.FuelRoutes.IntegrationEvents.csproj", "src/FuelRoute/Foruscorp.FuelRoutes.IntegrationEvents/"]
COPY ["src/FuelRoute/Foruscorp.FuelRoute.Infrastructure/Foruscorp.FuelRoutes.Infrastructure.csproj", "src/FuelRoute/Foruscorp.FuelRoute.Infrastructure/"]
RUN dotnet restore "./src/FuelRoute/Foruscorp.FuelRoute.API/Foruscorp.FuelRoutes.API.csproj"
COPY . .
WORKDIR "/src/src/FuelRoute/Foruscorp.FuelRoute.API"
RUN dotnet build "./Foruscorp.FuelRoutes.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Foruscorp.FuelRoutes.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Foruscorp.FuelRoutes.API.dll"]
# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FuelStations/Foruscorp.FuelStations.API/Foruscorp.FuelStations.API.csproj", "src/FuelStations/Foruscorp.FuelStations.API/"]
COPY ["src/FuelStations/Foruscorp.FuelStations.Aplication/Foruscorp.FuelStations.Aplication.csproj", "src/FuelStations/Foruscorp.FuelStations.Aplication/"]
COPY ["src/FuelStations/Foruscorp.FuelStations.Infrastructure/Foruscorp.FuelStations.Infrastructure.csproj", "src/FuelStations/Foruscorp.FuelStations.Infrastructure/"]
COPY ["src/FuelStations/Foruscorp.FuelStations.Domain/Foruscorp.FuelStations.Domain.csproj", "src/FuelStations/Foruscorp.FuelStations.Domain/"]
RUN dotnet restore "./src/FuelStations/Foruscorp.FuelStations.API/Foruscorp.FuelStations.API.csproj"
COPY . .
WORKDIR "/src/src/FuelStations/Foruscorp.FuelStations.API"
RUN dotnet build "./Foruscorp.FuelStations.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Foruscorp.FuelStations.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Foruscorp.FuelStations.API.dll"]
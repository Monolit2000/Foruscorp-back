# Скрипт для тестирования метрик Prometheus

Write-Host "Testing Prometheus Metrics..." -ForegroundColor Green

# Функция для проверки доступности эндпоинта
function Test-MetricsEndpoint {
    param(
        [string]$Url,
        [string]$ServiceName
    )
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ $ServiceName metrics endpoint is accessible" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ $ServiceName metrics endpoint returned status: $($response.StatusCode)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "✗ $ServiceName metrics endpoint is not accessible: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Функция для генерации нагрузки
function Invoke-LoadTest {
    param(
        [string]$BaseUrl,
        [string]$ServiceName,
        [int]$Requests = 10
    )
    
    Write-Host "Generating load for $ServiceName..." -ForegroundColor Yellow
    
    for ($i = 1; $i -le $Requests; $i++) {
        try {
            $testUrl = "$BaseUrl/api/metrics/test"
            $response = Invoke-WebRequest -Uri $testUrl -Method GET -TimeoutSec 5
            Write-Host "  Request $i/$Requests completed" -NoNewline
            Write-Host " (Status: $($response.StatusCode))" -ForegroundColor Gray
        }
        catch {
            Write-Host "  Request $i/$Requests failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Небольшая задержка между запросами
        Start-Sleep -Milliseconds 100
    }
}

# Проверяем доступность сервисов
$services = @(
    @{ Url = "http://localhost:5003"; Name = "Trucks API" },
    @{ Url = "http://localhost:5002"; Name = "FuelStations API" },
    @{ Url = "http://localhost:5004"; Name = "FuelRoutes API" },
    @{ Url = "http://localhost:5001"; Name = "TrucksTracking API" },
    @{ Url = "http://localhost:5007"; Name = "Auth API" },
    @{ Url = "http://localhost:5010"; Name = "Push API" },
    @{ Url = "http://localhost:5000"; Name = "Gateway" }
)

Write-Host "`nChecking metrics endpoints..." -ForegroundColor Cyan
$accessibleServices = @()

foreach ($service in $services) {
    $metricsUrl = "$($service.Url)/metrics"
    if (Test-MetricsEndpoint -Url $metricsUrl -ServiceName $service.Name) {
        $accessibleServices += $service
    }
}

# Проверяем Prometheus
Write-Host "`nChecking Prometheus..." -ForegroundColor Cyan
$prometheusUrl = "http://localhost:9090/api/v1/targets"
try {
    $response = Invoke-WebRequest -Uri $prometheusUrl -Method GET -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Prometheus is accessible" -ForegroundColor Green
        $targets = $response.Content | ConvertFrom-Json
        Write-Host "  Active targets: $($targets.data.activeTargets.Count)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Prometheus is not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

# Проверяем Grafana
Write-Host "`nChecking Grafana..." -ForegroundColor Cyan
$grafanaUrl = "http://localhost:3000/api/health"
try {
    $response = Invoke-WebRequest -Uri $grafanaUrl -Method GET -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Grafana is accessible" -ForegroundColor Green
        Write-Host "  Access Grafana at: http://localhost:3000 (admin/admin)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Grafana is not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

# Генерируем нагрузку для доступных сервисов
if ($accessibleServices.Count -gt 0) {
    Write-Host "`nGenerating test load..." -ForegroundColor Cyan
    foreach ($service in $accessibleServices) {
        Invoke-LoadTest -BaseUrl $service.Url -ServiceName $service.Name -Requests 5
    }
}

Write-Host "`nTest completed!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Open Prometheus: http://localhost:9090" -ForegroundColor White
Write-Host "2. Open Grafana: http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "3. Add Prometheus as data source in Grafana" -ForegroundColor White
Write-Host "4. Import dashboards from monitoring/grafana/dashboards/" -ForegroundColor White

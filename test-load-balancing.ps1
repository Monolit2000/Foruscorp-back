# Test Load Balancing for FuelRoutes API
# This script sends multiple requests to test round-robin load balancing

Write-Host "Testing Load Balancing for FuelRoutes API..." -ForegroundColor Green
Write-Host "Sending 10 requests to see distribution between replicas..." -ForegroundColor Yellow

$gatewayUrl = "http://localhost:5000/fuelroutes-api/health"
$requests = 10

for ($i = 1; $i -le $requests; $i++) {
    try {
        $response = Invoke-RestMethod -Uri $gatewayUrl -Method GET -TimeoutSec 5
        Write-Host "Request $i`: Success - Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Green
    }
    catch {
        Write-Host "Request $i`: Failed - $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Small delay between requests
    Start-Sleep -Milliseconds 500
}

Write-Host "`nLoad balancing test completed!" -ForegroundColor Green
Write-Host "Check the logs of both fuelroutes-api containers to see request distribution." -ForegroundColor Yellow

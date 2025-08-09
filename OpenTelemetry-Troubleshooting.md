# OpenTelemetry Troubleshooting Guide

## Проблема
При запросах через Gateway логи и метрики OpenTelemetry не собираются или не отображаются в системе мониторинга.

## Причины и решения

### 1. Отсутствие конфигурации логирования в Gateway

**Проблема**: Gateway не был настроен для отправки логов в OpenTelemetry.

**Решение**: Обновлена конфигурация Gateway:
```csharp
// Configure logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("YarpGateway"));
});
```

### 2. Отсутствие метрик в Gateway

**Проблема**: Gateway не собирал метрики.

**Решение**: Добавлена конфигурация метрик:
```csharp
.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddRuntimeInstrumentation()
    .AddMeter("YarpGateway"))
```

### 3. Проблемы с propagation trace context

**Проблема**: Trace context не передавался правильно между сервисами.

**Решение**: 
- Добавлен middleware для логирования trace context
- Улучшена конфигурация HttpClient instrumentation
- Добавлены дополнительные теги в трассировку

### 4. Отсутствие переменных окружения

**Проблема**: Переменные окружения для OpenTelemetry не были настроены.

**Решение**: Убедитесь, что в docker-compose.yml настроены:
```yaml
x-otel: &otel-env
  OTEL_EXPORTER_OTLP_ENDPOINT: "https://otlp-gateway-prod-us-west-0.grafana.net/otlp"
  OTEL_EXPORTER_OTLP_PROTOCOL: "http/protobuf"
  OTEL_EXPORTER_OTLP_HEADERS: "Authorization=Basic ..."
```

## Тестирование

### 1. Тест Gateway напрямую
```bash
# Тест Gateway telemetry
curl http://localhost:5000/api/telemetrytest/test
curl http://localhost:5000/api/telemetrytest/log-test
curl http://localhost:5000/api/telemetrytest/headers
```

### 2. Тест через Gateway к микросервису
```bash
# Тест Trucks API через Gateway
curl http://localhost:5000/trucks-api/api/telemetrytest/test
curl http://localhost:5000/trucks-api/api/telemetrytest/log-test
curl http://localhost:5000/trucks-api/api/telemetrytest/exception-test
```

### 3. Проверка trace context
```bash
# Проверка заголовков trace
curl -H "traceparent: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01" \
     http://localhost:5000/trucks-api/api/telemetrytest/test
```

## Диагностика

### 1. Проверка логов контейнеров
```bash
# Логи Gateway
docker logs foruscorp-gateway

# Логи Trucks API
docker logs trucks-api
```

### 2. Проверка переменных окружения
```bash
# Проверка переменных в Gateway
docker exec foruscorp-gateway env | grep OTEL

# Проверка переменных в Trucks API
docker exec trucks-api env | grep OTEL
```

### 3. Проверка сетевого подключения
```bash
# Проверка доступности OTLP endpoint
docker exec foruscorp-gateway curl -I https://otlp-gateway-prod-us-west-0.grafana.net/otlp
```

## Мониторинг

### 1. Grafana Cloud
- Проверьте логи в Grafana Cloud
- Проверьте метрики в Grafana Cloud
- Проверьте трассировку в Grafana Cloud

### 2. Локальные логи
```bash
# Просмотр логов в реальном времени
docker logs -f foruscorp-gateway
docker logs -f trucks-api
```

## Дополнительные настройки

### 1. Уровни логирования
В `appsettings.json` каждого сервиса:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Yarp": "Information",
      "OpenTelemetry": "Debug"
    }
  }
}
```

### 2. Sampling
Для уменьшения объема данных можно настроить sampling:
```csharp
.WithTracing(tracing => tracing
    .AddAspNetCoreInstrumentation()
    .SetSampler(new TraceIdRatioBasedSampler(0.1)) // 10% sampling
    .UseOtlpExporter())
```

### 3. Resource attributes
Добавьте дополнительные атрибуты для лучшей идентификации:
```csharp
.ConfigureResource(resource => resource
    .AddService("Trucks.API")
    .AddAttributes(new KeyValuePair<string, object>[]
    {
        new("service.instance.id", Environment.MachineName),
        new("service.version", "1.0.0"),
        new("deployment.environment", "production")
    }))
```

## Частые проблемы

### 1. "No traces visible"
- Проверьте переменные окружения OTEL_EXPORTER_OTLP_*
- Убедитесь, что endpoint доступен
- Проверьте аутентификацию

### 2. "Logs not appearing"
- Проверьте конфигурацию логирования
- Убедитесь, что AddOpenTelemetry() вызван для логирования
- Проверьте уровни логирования

### 3. "Metrics not showing"
- Проверьте конфигурацию метрик
- Убедитесь, что AddMeter() настроен правильно
- Проверьте, что метрики экспортируются

### 4. "Trace context not propagating"
- Проверьте заголовки traceparent и tracestate
- Убедитесь, что HttpClient instrumentation настроен
- Проверьте middleware для обработки trace context

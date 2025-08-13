# 🚀 Мониторинг Foruscorp с Grafana и Prometheus

Полная система мониторинга для всех .NET приложений и инфраструктурных сервисов.

## 📊 Что включено

### Компоненты мониторинга
- **Prometheus** - сбор и хранение метрик
- **Grafana** - визуализация и дашборды
- **Алерты** - автоматические уведомления о проблемах
- **Кастомные метрики** - бизнес-логика метрики

### Мониторимые сервисы
- ✅ Trucks API (порт 5003)
- ✅ FuelStations API (порт 5002)
- ✅ FuelRoutes API (порт 5004)
- ✅ TrucksTracking API (порт 5001)
- ✅ Auth API (порт 5007)
- ✅ Push API (порт 5010)
- ✅ Gateway (порт 5000)
- ✅ PostgreSQL
- ✅ Redis
- ✅ RabbitMQ

## 🚀 Быстрый старт

### 1. Запуск системы
```bash
# Запуск всех сервисов
docker-compose up -d

# Проверка статуса
docker-compose ps
```

### 2. Доступ к интерфейсам

| Сервис | URL | Логин/Пароль |
|--------|-----|--------------|
| **Prometheus** | http://localhost:9090 | - |
| **Grafana** | http://localhost:3000 | admin/admin |
| **RabbitMQ Management** | http://localhost:15672 | guest/guest |

### 3. Тестирование метрик
```powershell
# Запуск тестового скрипта
.\monitoring\test-metrics.ps1
```

## 📈 Доступные метрики

### HTTP метрики
- Количество запросов (`http_requests_total`)
- Время ответа (`http_request_duration_seconds`)
- Статус коды
- Размер запросов/ответов

### Runtime метрики
- Использование CPU (`process_cpu_seconds_total`)
- Использование памяти (`process_resident_memory_bytes`)
- Количество потоков (`process_open_handles`)
- GC статистика

### Database метрики
- Время выполнения запросов
- Количество активных соединений
- Размер базы данных

### Кастомные метрики
- Бизнес-операции (`trucks_api_business_operations_total`)
- Активные соединения (`trucks_api_active_connections`)
- Время выполнения операций

## 📊 Дашборды

### 1. .NET Applications Overview
**Файл**: `monitoring/grafana/dashboards/dotnet-applications.json`

Метрики:
- HTTP Request Rate
- HTTP Response Time (95th percentile)
- Memory Usage
- CPU Usage
- Active Connections

### 2. Infrastructure Overview
**Файл**: `monitoring/grafana/dashboards/infrastructure.json`

Метрики:
- Database Connections
- Redis Memory Usage
- RabbitMQ Queue Messages
- Database Query Duration

## 🔔 Алерты

Система автоматически отслеживает:

### Критические алерты
- ❌ Сервис недоступен (`ServiceDown`)
- ❌ Высокая частота ошибок (`HighErrorRate`)

### Предупреждения
- ⚠️ Высокое использование CPU (`HighCPUUsage`)
- ⚠️ Высокое использование памяти (`HighMemoryUsage`)
- ⚠️ Медленные HTTP ответы (`HighHTTPResponseTime`)
- ⚠️ Много соединений к БД (`HighDatabaseConnections`)
- ⚠️ Высокое использование Redis (`HighRedisMemoryUsage`)
- ⚠️ Много сообщений в RabbitMQ (`HighRabbitMQQueueMessages`)

## 🛠️ Настройка

### Добавление новых метрик

#### В .NET приложении
```csharp
using Prometheus;

// Счетчик
var counter = Metrics.CreateCounter("my_counter", "Description");
counter.Inc();

// Гистограмма
var histogram = Metrics.CreateHistogram("my_histogram", "Description");
histogram.Observe(value);

// Gauge
var gauge = Metrics.CreateGauge("my_gauge", "Description");
gauge.Set(value);
```

#### В Prometheus конфигурации
```yaml
# monitoring/prometheus/prometheus.yml
- job_name: 'new-service'
  static_configs:
    - targets: ['new-service:port']
  metrics_path: '/metrics'
  scrape_interval: 10s
```

### Создание нового дашборда

1. Создайте JSON файл в `monitoring/grafana/dashboards/`
2. Импортируйте в Grafana через UI
3. Или добавьте в `monitoring/grafana/provisioning/dashboards/dashboards.yml`

## 🔍 Полезные запросы Prometheus

### Производительность
```promql
# HTTP Request Rate
rate(http_requests_total[5m])

# 95th percentile response time
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Error rate
rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m])
```

### Ресурсы
```promql
# Memory usage in MB
process_resident_memory_bytes / 1024 / 1024

# CPU usage percentage
rate(process_cpu_seconds_total[5m]) * 100

# Active connections
process_open_handles
```

### Бизнес-метрики
```promql
# Business operations rate
rate(trucks_api_business_operations_total[5m])

# Success vs Error ratio
rate(trucks_api_business_operations_total{status="success"}[5m]) / 
rate(trucks_api_business_operations_total[5m])
```

## 🧪 Тестирование

### Ручное тестирование метрик
```bash
# Проверка метрик Trucks API
curl http://localhost:5003/metrics

# Проверка health check
curl http://localhost:5003/health

# Тестовый эндпоинт с метриками
curl http://localhost:5003/api/metrics/test
```

### Генерация нагрузки
```bash
# POST запрос для симуляции нагрузки
curl -X POST http://localhost:5003/api/metrics/simulate-load \
  -H "Content-Type: application/json" \
  -d '{"delayMs": 100, "errorRate": 5}'
```

## 🔧 Troubleshooting

### Проблемы с доступностью метрик
1. Проверьте, что приложение запущено
2. Убедитесь, что порт открыт
3. Проверьте логи приложения

### Проблемы с Prometheus
```bash
# Проверка статуса targets
curl http://localhost:9090/api/v1/targets

# Проверка конфигурации
curl http://localhost:9090/api/v1/status/config
```

### Проблемы с Grafana
```bash
# Проверка health
curl http://localhost:3000/api/health

# Проверка источников данных
curl http://localhost:3000/api/datasources
```

## 🔒 Безопасность

### Рекомендации для продакшена
1. **Измените пароли по умолчанию**
   ```bash
   # В docker-compose.yml
   GF_SECURITY_ADMIN_PASSWORD: your_secure_password
   ```

2. **Настройте аутентификацию**
   - LDAP/Active Directory
   - OAuth2
   - SAML

3. **Используйте HTTPS**
   - Настройте SSL сертификаты
   - Используйте reverse proxy

4. **Ограничьте доступ**
   - Настройте firewall
   - Используйте VPN
   - Ограничьте IP адреса

## 📚 Дополнительные ресурсы

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [prometheus-net Documentation](https://github.com/prometheus-net/prometheus-net)
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)

## 🤝 Поддержка

При возникновении проблем:
1. Проверьте логи: `docker-compose logs [service-name]`
2. Запустите тестовый скрипт: `.\monitoring\test-metrics.ps1`
3. Проверьте конфигурацию в `monitoring/` папке
4. Убедитесь, что все порты свободны

---

**🎉 Готово!** Ваша система мониторинга настроена и готова к использованию.

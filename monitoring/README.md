# Мониторинг с Grafana и Prometheus

Этот проект настроен для мониторинга всех .NET приложений с использованием Prometheus и Grafana.

## Компоненты

### Prometheus
- **Порт**: 9090
- **URL**: http://localhost:9090
- **Функции**: Сбор метрик со всех сервисов

### Grafana
- **Порт**: 3000
- **URL**: http://localhost:3000
- **Логин**: admin
- **Пароль**: admin
- **Функции**: Визуализация метрик и дашборды

## Запуск

1. Убедитесь, что Docker и Docker Compose установлены
2. Запустите все сервисы:
   ```bash
   docker-compose up -d
   ```

## Доступные метрики

### .NET Приложения
Все API сервисы теперь экспортируют метрики на эндпоинте `/metrics`:

- **HTTP метрики**: Количество запросов, время ответа, статус коды
- **Runtime метрики**: Использование CPU, памяти, GC статистика
- **Database метрики**: Время выполнения запросов к PostgreSQL
- **Custom метрики**: Бизнес-логика метрики

### Инфраструктурные сервисы
- **RabbitMQ**: Очереди, сообщения, соединения
- **Redis**: Использование памяти, количество ключей
- **PostgreSQL**: Активные соединения, время выполнения запросов

## Дашборды

### .NET Applications Overview
Базовый дашборд с основными метриками:
- HTTP Request Rate
- HTTP Response Time
- Memory Usage
- CPU Usage
- Active Connections

## Настройка

### Prometheus
Конфигурация находится в `monitoring/prometheus/prometheus.yml`

### Grafana
- Источники данных: `monitoring/grafana/provisioning/datasources/`
- Дашборды: `monitoring/grafana/provisioning/dashboards/`

## Добавление новых метрик

### В .NET приложении
```csharp
// Создание счетчика
var counter = Metrics.CreateCounter("my_counter", "Description");

// Увеличение счетчика
counter.Inc();

// Создание гистограммы
var histogram = Metrics.CreateHistogram("my_histogram", "Description");

// Запись значения
histogram.Observe(value);
```

### В Prometheus конфигурации
Добавьте новый job в `prometheus.yml`:
```yaml
- job_name: 'new-service'
  static_configs:
    - targets: ['new-service:port']
  metrics_path: '/metrics'
  scrape_interval: 10s
```

## Полезные запросы Prometheus

### HTTP Request Rate
```
rate(http_requests_total[5m])
```

### 95-й процентиль времени ответа
```
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

### Использование памяти
```
process_resident_memory_bytes / 1024 / 1024
```

### Использование CPU
```
rate(process_cpu_seconds_total[5m]) * 100
```

## Troubleshooting

### Проверка доступности метрик
```bash
curl http://localhost:5003/metrics  # Trucks API
curl http://localhost:5002/metrics  # FuelStations API
curl http://localhost:5004/metrics  # FuelRoutes API
```

### Проверка Prometheus
```bash
curl http://localhost:9090/api/v1/targets
```

### Проверка Grafana
```bash
curl http://localhost:3000/api/health
```

## Безопасность

В продакшене рекомендуется:
1. Изменить пароли по умолчанию
2. Настроить аутентификацию
3. Использовать HTTPS
4. Ограничить доступ к метрикам

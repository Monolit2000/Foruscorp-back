# Foruscorp Kubernetes Deployment

Этот репозиторий содержит Kubernetes манифесты для развертывания приложения Foruscorp.

## Структура проекта

```
k8s/
├── namespace.yaml              # Namespace для приложения
├── configmap.yaml              # Общие настройки
├── secrets.yaml                # Секретные данные
├── postgres.yaml               # PostgreSQL база данных
├── rabbitmq.yaml               # RabbitMQ message broker
├── redis.yaml                  # Redis кэш
├── gateway.yaml                # API Gateway
├── auth-api.yaml               # Auth API сервис
├── push-api.yaml               # Push API сервис
├── fuelstations-api.yaml       # Fuel Stations API
├── trucks-api.yaml             # Trucks API
├── truckstracking-api.yaml     # Trucks Tracking API
├── fuelroutes-api.yaml         # Fuel Routes API
├── aspire-dashboard.yaml       # Aspire Dashboard
├── ingress.yaml                # Ingress контроллер
├── hpa.yaml                    # Horizontal Pod Autoscaler
├── network-policy.yaml         # Network Policy
├── deploy-all.yaml             # Общий файл для развертывания
└── README.md                   # Этот файл
```

## Предварительные требования

1. **Kubernetes кластер** (версия 1.20+)
2. **kubectl** настроенный для работы с кластером
3. **Helm** (опционально, для установки дополнительных компонентов)
4. **NGINX Ingress Controller** или другой Ingress контроллер
5. **cert-manager** (для SSL сертификатов)

## Установка

### 1. Создание namespace и базовых ресурсов

```bash
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f secrets.yaml
```

### 2. Развертывание инфраструктурных сервисов

```bash
kubectl apply -f postgres.yaml
kubectl apply -f rabbitmq.yaml
kubectl apply -f redis.yaml
```

### 3. Развертывание приложений

```bash
kubectl apply -f gateway.yaml
kubectl apply -f auth-api.yaml
kubectl apply -f push-api.yaml
kubectl apply -f fuelstations-api.yaml
kubectl apply -f trucks-api.yaml
kubectl apply -f truckstracking-api.yaml
kubectl apply -f fuelroutes-api.yaml
kubectl apply -f aspire-dashboard.yaml
```

### 4. Настройка сетевого доступа

```bash
kubectl apply -f ingress.yaml
kubectl apply -f network-policy.yaml
```

### 5. Настройка автомасштабирования

```bash
kubectl apply -f hpa.yaml
```

### Альтернативный способ - развертывание всего сразу

```bash
kubectl apply -f deploy-all.yaml
```

## Настройка

### 1. Обновление секретов

Перед развертыванием обновите файл `secrets.yaml`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: foruscorp-secrets
  namespace: foruscorp
type: Opaque
data:
  postgres-password: <base64-encoded-password>
  samsara-api-token: <base64-encoded-token>
  certificate-password: <base64-encoded-cert-password>
```

### 2. Настройка SSL сертификатов

Обновите файл `gateway.yaml` или создайте отдельный Secret:

```bash
kubectl create secret tls foruscorp-certificate \
  --cert=path/to/certificate.crt \
  --key=path/to/private.key \
  --namespace=foruscorp
```

### 3. Настройка доменного имени

Обновите файл `ingress.yaml` с вашим доменным именем:

```yaml
spec:
  tls:
  - hosts:
    - your-domain.com
    secretName: foruscorp-tls
  rules:
  - host: your-domain.com
```

## Мониторинг и логирование

### 1. Aspire Dashboard

Aspire Dashboard доступен по адресу: `https://your-domain.com/dashboard`

### 2. RabbitMQ Management

RabbitMQ Management доступен внутри кластера по адресу: `http://rabbitmq:15672`

### 3. Проверка статуса

```bash
# Проверка всех подов
kubectl get pods -n foruscorp

# Проверка сервисов
kubectl get services -n foruscorp

# Проверка ingress
kubectl get ingress -n foruscorp

# Логи конкретного пода
kubectl logs -f deployment/foruscorp-gateway -n foruscorp
```

## Масштабирование

### Автоматическое масштабирование

HPA уже настроен для основных сервисов. Проверьте статус:

```bash
kubectl get hpa -n foruscorp
```

### Ручное масштабирование

```bash
kubectl scale deployment foruscorp-gateway --replicas=5 -n foruscorp
```

## Обновление приложения

### 1. Обновление образа

```bash
kubectl set image deployment/foruscorp-gateway gateway=new-image:tag -n foruscorp
```

### 2. Rolling update

```bash
kubectl rollout restart deployment/foruscorp-gateway -n foruscorp
```

### 3. Проверка статуса обновления

```bash
kubectl rollout status deployment/foruscorp-gateway -n foruscorp
```

## Удаление

### Удаление всего приложения

```bash
kubectl delete namespace foruscorp
```

### Удаление отдельных компонентов

```bash
kubectl delete -f gateway.yaml
kubectl delete -f auth-api.yaml
# и т.д.
```

## Troubleshooting

### 1. Поды не запускаются

```bash
# Проверка событий
kubectl describe pod <pod-name> -n foruscorp

# Проверка логов
kubectl logs <pod-name> -n foruscorp
```

### 2. Проблемы с подключением к базе данных

```bash
# Проверка статуса PostgreSQL
kubectl get pods -l app=foruscorp-postgres -n foruscorp

# Проверка логов PostgreSQL
kubectl logs -l app=foruscorp-postgres -n foruscorp
```

### 3. Проблемы с RabbitMQ

```bash
# Проверка статуса RabbitMQ
kubectl get pods -l app=rabbitmq -n foruscorp

# Проверка логов RabbitMQ
kubectl logs -l app=rabbitmq -n foruscorp
```

## Безопасность

### 1. Network Policies

Network Policies настроены для ограничения трафика между сервисами.

### 2. Secrets

Все чувствительные данные хранятся в Kubernetes Secrets.

### 3. RBAC

Для продакшена рекомендуется настроить RBAC для ограничения доступа.

## Производительность

### 1. Resource Limits

Все поды имеют настроенные лимиты ресурсов.

### 2. Horizontal Pod Autoscaler

HPA настроен для автоматического масштабирования на основе CPU и памяти.

### 3. Persistent Volumes

База данных и кэш используют Persistent Volumes для сохранения данных.

## Поддержка

При возникновении проблем:

1. Проверьте логи приложений
2. Проверьте статус подов и сервисов
3. Проверьте события в namespace
4. Обратитесь к документации Kubernetes

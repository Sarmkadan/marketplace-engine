// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

Production deployment strategies for Marketplace Engine.

## Deployment Architecture

```
┌──────────────────────────────────────────────────────┐
│               Client Applications                     │
└────────────────────┬─────────────────────────────────┘
                     │
┌────────────────────▼─────────────────────────────────┐
│            Load Balancer / Reverse Proxy              │
│           (nginx / HAProxy / Azure LB)                │
└────────────────────┬─────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
┌───────▼────┐ ┌─────▼──────┐ ┌──▼────────┐
│ Instance 1 │ │ Instance 2 │ │ Instance 3│
└────────────┘ └────────────┘ └───────────┘
        │            │            │
        └────────────┼────────────┘
                     │
┌────────────────────▼─────────────────────────────────┐
│              Shared Cache (Redis)                     │
└────────────────────────────────────────────────────────┘
        │
        ├──────────────────────┐
        │                      │
┌───────▼────────┐  ┌─────────▼────────┐
│  Database      │  │  Backup DB       │
│  (Primary)     │  │  (Replica)       │
└────────────────┘  └──────────────────┘
```

## Local Development

### Prerequisites

- .NET 10 SDK
- Windows / macOS / Linux
- Visual Studio Code or Visual Studio 2022

### Setup

```bash
git clone https://github.com/Sarmkadan/marketplace-engine.git
cd marketplace-engine
dotnet restore
dotnet build
```

### Running

```bash
dotnet run --project src/MarketplaceEngine
```

Access at: `http://localhost:5000`

---

## Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnetcore:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000 5001
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "MarketplaceEngine.dll"]
```

### Build Docker Image

```bash
docker build -t marketplace-engine:latest .
```

### Run Docker Container

```bash
docker run -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  marketplace-engine:latest
```

### Docker Compose

```yaml
version: '3.8'
services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5000
    depends_on:
      - cache
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/api/v1/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  cache:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 3
```

### Start with Docker Compose

```bash
docker-compose up -d
```

---

## Azure Deployment

### Prerequisites

- Azure account
- Azure CLI installed
- Docker image pushed to Azure Container Registry

### 1. Create Resource Group

```bash
az group create \
  --name marketplace-rg \
  --location eastus
```

### 2. Create App Service Plan

```bash
az appservice plan create \
  --name marketplace-plan \
  --resource-group marketplace-rg \
  --sku B2 \
  --is-linux
```

### 3. Create Web App

```bash
az webapp create \
  --resource-group marketplace-rg \
  --plan marketplace-plan \
  --name marketplace-api \
  --deployment-container-image-name-user marketplace-engine:latest
```

### 4. Configure Application Settings

```bash
az webapp config appsettings set \
  --resource-group marketplace-rg \
  --name marketplace-api \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

### 5. Deploy Application

```bash
az webapp deployment container config \
  --name marketplace-api \
  --resource-group marketplace-rg \
  --enable-cd
```

---

## AWS Deployment

### Using Elastic Container Service (ECS)

#### 1. Create ECS Cluster

```bash
aws ecs create-cluster --cluster-name marketplace
```

#### 2. Register Task Definition

```bash
aws ecs register-task-definition \
  --family marketplace-api \
  --network-mode awsvpc \
  --requires-compatibilities FARGATE \
  --cpu 256 \
  --memory 512 \
  --container-definitions '[
    {
      "name": "marketplace",
      "image": "YOUR_ECR_URI:latest",
      "portMappings": [{"containerPort": 5000}]
    }
  ]'
```

#### 3. Create Service

```bash
aws ecs create-service \
  --cluster marketplace \
  --service-name marketplace-api \
  --task-definition marketplace-api \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx],securityGroups=[sg-xxx]}"
```

---

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (Azure AKS, AWS EKS, or local kind)
- kubectl installed
- Docker image in registry

### Deployment Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: marketplace-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: marketplace-api
  template:
    metadata:
      labels:
        app: marketplace-api
    spec:
      containers:
      - name: api
        image: marketplace-engine:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ASPNETCORE_URLS
          value: http://+:5000
        livenessProbe:
          httpGet:
            path: /api/v1/health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /api/v1/health
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 10
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
---
apiVersion: v1
kind: Service
metadata:
  name: marketplace-api-service
spec:
  type: LoadBalancer
  selector:
    app: marketplace-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5000
```

### Deploy to Kubernetes

```bash
kubectl apply -f deployment.yaml
kubectl get deployments
kubectl get services
```

---

## Production Configuration

### appsettings.Production.json

```json
{
  "MarketplaceConfiguration": {
    "MaxListingsPerUser": 500,
    "MaxMessageLengthPerMessage": 5000,
    "EnableModeration": true,
    "MaxDaysToRetainMessages": 365,
    "RateLimitRequestsPerMinute": 100,
    "AllowedCurrencies": ["USD", "EUR", "GBP", "JPY"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "MarketplaceEngine": "Information"
    }
  },
  "Database": {
    "ConnectionString": "Server=prod-db.example.com;Database=marketplace;User=sa;Password=***;",
    "CommandTimeout": 30,
    "MaxPoolSize": 100
  },
  "Cache": {
    "RedisConnection": "prod-cache.example.com:6379",
    "DefaultTTL": 3600
  },
  "Security": {
    "JwtSecret": "***PRODUCTION_SECRET***",
    "JwtExpirationMinutes": 60,
    "RequireHttps": true
  }
}
```

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
DATABASE_CONNECTION_STRING=Server=...
REDIS_CONNECTION_STRING=...
JWT_SECRET=...
LOG_LEVEL=Warning
```

---

## Monitoring & Logging

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### ELK Stack (Elasticsearch, Logstash, Kibana)

```csharp
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
        {
            IndexFormat = "marketplace-{0:yyyy.MM.dd}"
        }));
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", databaseHealthCheck)
    .AddCheck("cache", cacheHealthCheck);
```

---

## Scaling Strategies

### Vertical Scaling

Increase resources per instance:
- Add CPU cores
- Increase RAM
- Use faster storage

### Horizontal Scaling

Add more instances:
- Auto-scaling based on CPU/memory
- Load balancer distribution
- Stateless design required

### Caching Strategy

- Redis for distributed cache
- CDN for static assets
- Cache-aside pattern

### Database Optimization

- Query optimization with indexes
- Read replicas for scaling reads
- Partitioning for large tables

---

## Security Checklist

- [ ] Enable HTTPS/TLS
- [ ] Implement rate limiting
- [ ] Use environment variables for secrets
- [ ] Enable API authentication (JWT)
- [ ] Implement CORS properly
- [ ] Use strong passwords in database
- [ ] Enable database encryption
- [ ] Set up Web Application Firewall (WAF)
- [ ] Regular security audits
- [ ] Keep dependencies updated
- [ ] Implement logging and monitoring
- [ ] Set up backup strategy

---

## Backup Strategy

### Database Backups

```bash
# Daily automated backups
0 2 * * * mysqldump -u user -p database > /backups/db-$(date +\%Y\%m\%d).sql

# Weekly full backup
0 0 * * 0 tar czf /backups/full-backup-$(date +\%Y\%m\%d).tar.gz /app /data
```

### Retention Policy

- Daily backups: 7 days
- Weekly backups: 4 weeks
- Monthly backups: 1 year

---

## Zero-Downtime Deployment

### Blue-Green Deployment

```bash
# Deploy to green environment
kubectl apply -f deployment-green.yaml

# Test green environment
kubectl run test-pod --image=curlimages/curl -- curl http://marketplace-api-green/health

# Switch traffic
kubectl patch service marketplace-api-service -p '{"spec":{"selector":{"version":"green"}}}'

# Keep blue as rollback
```

### Canary Deployment

```yaml
apiVersion: flagger.app/v1beta1
kind: Canary
metadata:
  name: marketplace-api
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: marketplace-api
  progressDeadlineSeconds: 60
  service:
    port: 80
  analysis:
    interval: 1m
    threshold: 5
    metrics:
    - name: request-success-rate
      thresholdRange:
        min: 99
    maxWeight: 50
    stepWeight: 10
```

---

## Rollback Procedure

### Docker

```bash
# Rollback to previous version
docker run -p 5000:5000 marketplace-engine:v1.0.0

# Or with docker-compose
docker-compose down
git checkout v1.0.0
docker-compose up
```

### Kubernetes

```bash
kubectl rollout history deployment/marketplace-api
kubectl rollout undo deployment/marketplace-api
kubectl rollout undo deployment/marketplace-api --to-revision=2
```

---

## Performance Tuning

### Database Connection Pooling

```json
"ConnectionString": "Server=...;MaxPoolSize=100;MinPoolSize=10;"
```

### ASP.NET Core Optimization

```csharp
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = false;
    options.Limits.MaxRequestBodySize = 10_485_760; // 10MB
});
```

### Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
app.UseResponseCompression();
```

---

## Troubleshooting

### High CPU Usage
1. Check slow queries in logs
2. Add database indexes
3. Scale horizontally
4. Profile application

### High Memory Usage
1. Check for memory leaks
2. Enable garbage collection tuning
3. Reduce cache TTL
4. Scale up

### Database Connection Timeout
1. Check connection string
2. Increase pool size
3. Check database health
4. Review slow queries

### API Slowness
1. Check endpoint response times
2. Analyze slow queries
3. Check cache hit rate
4. Review rate limiting

---

**Next Steps:**
- Review [FAQ](faq.md) for common questions
- Check [Architecture](architecture.md) for system design
- See [API Reference](api-reference.md) for endpoints

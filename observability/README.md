# Observability Stack (FastAPI-Base Style)

Centralized monitoring for the Employee Management System.

## Access Links
- **Grafana**: [http://localhost:3000](http://localhost:3000) (User: `admin` / Pass: `admin`)
- **Loki (Logs)**: [http://localhost:3100](http://localhost:3100)/ready
- **Tempo (Traces)**: [http://localhost:3200](http://localhost:3200)
- **Prometheus**: [http://localhost:9090](http://localhost:9090)

## Configuration Files
Aligned with the `fastapi-base` repository structure:
- `grafana-datasources.yaml`: Auto-provisions Loki, Tempo, and Prometheus in Grafana.
- `loki-config.yaml`: Custom Loki log storage and retention.
- `otel-collector-config.yaml`: Centralized OTLP collection.
- `prometheus.yaml`: Scrape configuration.
- `tempo.yaml`: Trace storage settings.

## Troubleshooting
If you cannot open the links:
1. Ensure the container is running: `docker ps | grep employee_grafana`
2. Restart the stack to apply port mappings: `docker-compose up -d --force-recreate`
3. Check internal logs: `docker logs employee_grafana`

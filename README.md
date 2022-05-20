# Endpoint availability monitoring to app insights

Endpoint availability monitoring to app insights.

## Build Status

[![Build Status](https://dev.azure.com/jannemattila/jannemattila/_apis/build/status/JanneMattila.endpoint-availability-monitoring-to-app-insights?branchName=main)](https://dev.azure.com/jannemattila/jannemattila/_build/latest?definitionId=65&branchName=main)
![Docker Pulls](https://img.shields.io/docker/pulls/jannemattila/endpoint-availability-to-app-insights?style=plastic)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Prepare Azure infra

```bash
az extension add -n application-insights

workspace_name="log-monitoring"
ai_name="ai-endpoint"
location="northeurope"
resource_group="rg-endpoint-monitoring"

az group create -l $location -n $resource_group -o table


workspace_id=$(az monitor log-analytics workspace create -g $resource_group -n $workspace_name --query id -o tsv)
echo $workspace_id

ai_json=$(az monitor app-insights component create --app $ai_name --location $location --kind web -g $resource_group --workspace $workspace_id -o json)
ai_connectionstring=$(echo $ai_json | jq -r .connectionString)
echo $ai_connectionstring
```

## Configuration

You can configure endpoints to monitor in `appsettings.json`:

```json
{
  "uris": [
    "https://bing.com",
    "https://docker.io"
  ],
  "frequency": 60,
  "location": "Office",
  "connectionstring":  "<put your app insights connectionstring here>"
}
```

## App Insights

You should see availability monitoring results in your app insights availability tests view:

![Availability tests view](https://user-images.githubusercontent.com/2357647/169524744-018d7673-04b7-4ddb-88e9-4d5440ae344c.png)

Furthermore, you can configure alerting in Azure in case availability failures are reported
by the endpoint monitoring.

## Deployment

You can deploy this example application to Kubernetes using following configuration:

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: monitoring
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: monitoring
  namespace: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: monitoring
  template:
    metadata:
      labels:
        app: monitoring
    spec:
      nodeSelector:
        kubernetes.io/os: linux
      containers:
      - image: jannemattila/endpoint-availability-to-app-insights
        name: monitoring
        env:
        - name: uris__0
          value: "https://bing.com"
        - name: uris__1
          value: "https://github.com"
        - name: connectionstring
          value: "<your app insights connectionstring>"
```

# Endpoint availability monitoring to app insights

Endpoint availability monitoring to app insights.

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

#!/usr/bin/env bash
# =============================================================================
# Flowingly - Azure Deployment Script
# =============================================================================
# Provisions:
#   - Resource Group
#   - Azure Container Registry (ACR) - Basic tier
#   - Container Apps Environment
#   - Container App: API (.NET 8)
#   - Static Web App: Frontend (React)
#
# Prerequisites:
#   - Azure CLI installed and logged in (az login)
#   - Docker running locally (for initial image push)
#
# Usage:
#   chmod +x infra/deploy.sh
#   ./infra/deploy.sh
#
# After first run, CI/CD via GitHub Actions handles subsequent deployments.
# =============================================================================

set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration - edit these values before running
# ---------------------------------------------------------------------------
RESOURCE_GROUP="flowingly-rg"
LOCATION="australiaeast"           # Change to your preferred region
ACR_NAME="flowinglyacr"            # Must be globally unique, lowercase, 5-50 chars
CONTAINER_ENV="flowingly-env"
API_APP_NAME="flowingly-api"
STATIC_APP_NAME="flowingly-frontend"

# ---------------------------------------------------------------------------
# Derived values
# ---------------------------------------------------------------------------
ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"
API_IMAGE="${ACR_LOGIN_SERVER}/flowingly-api:latest"

echo "==> Creating resource group: ${RESOURCE_GROUP}"
az group create \
  --name "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --output none

# ---------------------------------------------------------------------------
# Azure Container Registry
# ---------------------------------------------------------------------------
echo "==> Creating Container Registry: ${ACR_NAME}"
az acr create \
  --resource-group "${RESOURCE_GROUP}" \
  --name "${ACR_NAME}" \
  --sku Basic \
  --admin-enabled true \
  --output none

echo "==> Logging in to ACR"
az acr login --name "${ACR_NAME}"

# ---------------------------------------------------------------------------
# Build and push API image
# ---------------------------------------------------------------------------
echo "==> Building and pushing API image"
docker build \
  --tag "${API_IMAGE}" \
  --file backend/Dockerfile \
  backend/

docker push "${API_IMAGE}"

# ---------------------------------------------------------------------------
# Container Apps Environment
# ---------------------------------------------------------------------------
echo "==> Creating Container Apps Environment: ${CONTAINER_ENV}"
az containerapp env create \
  --name "${CONTAINER_ENV}" \
  --resource-group "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --output none

# ---------------------------------------------------------------------------
# API Container App
# ---------------------------------------------------------------------------
ACR_PASSWORD=$(az acr credential show \
  --name "${ACR_NAME}" \
  --query "passwords[0].value" \
  --output tsv)

echo "==> Creating API Container App: ${API_APP_NAME}"
az containerapp create \
  --name "${API_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --environment "${CONTAINER_ENV}" \
  --image "${API_IMAGE}" \
  --registry-server "${ACR_LOGIN_SERVER}" \
  --registry-username "${ACR_NAME}" \
  --registry-password "${ACR_PASSWORD}" \
  --target-port 5000 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 1 \
  --env-vars \
      ASPNETCORE_URLS="http://+:5000" \
      ASPNETCORE_ENVIRONMENT="Production" \
  --output none

API_URL=$(az containerapp show \
  --name "${API_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --query "properties.configuration.ingress.fqdn" \
  --output tsv)

echo "==> API deployed at: https://${API_URL}"

# ---------------------------------------------------------------------------
# Static Web App (frontend)
# ---------------------------------------------------------------------------
echo "==> Creating Static Web App: ${STATIC_APP_NAME}"
az staticwebapp create \
  --name "${STATIC_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --sku Free \
  --output none

STATIC_URL=$(az staticwebapp show \
  --name "${STATIC_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --query "defaultHostname" \
  --output tsv)

echo ""
echo "============================================================"
echo "  Deployment complete!"
echo "============================================================"
echo "  API URL:      https://${API_URL}"
echo "  Frontend URL: https://${STATIC_URL}"
echo ""
echo "  Next steps:"
echo "  1. Add the following secrets to your GitHub repository:"
echo "     AZURE_CREDENTIALS        - Service principal JSON (see infra/README.md)"
echo "     ACR_LOGIN_SERVER         - ${ACR_LOGIN_SERVER}"
echo "     ACR_USERNAME             - ${ACR_NAME}"
echo "     ACR_PASSWORD             - (from: az acr credential show --name ${ACR_NAME})"
echo "     AZURE_RESOURCE_GROUP     - ${RESOURCE_GROUP}"
echo "     AZURE_CONTAINER_APP_NAME - ${API_APP_NAME}"
echo "     AZURE_STATIC_APP_TOKEN   - (from: az staticwebapp secrets list --name ${STATIC_APP_NAME} --query 'properties.apiKey' -o tsv)"
echo "     VITE_API_URL             - https://${API_URL}"
echo "  2. Push to main to trigger the CI/CD pipeline."
echo "============================================================"

#!/usr/bin/env bash
# =============================================================================
# Flowingly - Azure Infrastructure Provisioning Script
# =============================================================================
# Provisions:
#   - Resource Group
#   - Azure Container Registry (ACR) - Basic tier
#   - Container Apps Environment
#   - Container App: API (.NET 8) - initially with a placeholder image
#   - Static Web App: Frontend (React)
#
# NOTE: This script only provisions infrastructure.
# The first real image build and deploy happens via GitHub Actions
# after you add the secrets printed at the end of this script.
#
# Prerequisites:
#   - Azure CLI installed and logged in (az login)
#
# Usage:
#   bash infra/deploy.sh
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

# ---------------------------------------------------------------------------
# Register required providers and wait for all to complete
# ---------------------------------------------------------------------------
echo "==> Registering required Azure providers (this may take a minute)"
az provider register --namespace Microsoft.ContainerRegistry
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
az provider register --namespace Microsoft.Web

echo "==> Waiting for Microsoft.ContainerRegistry..."
az provider register --namespace Microsoft.ContainerRegistry --wait
echo "==> Waiting for Microsoft.App..."
az provider register --namespace Microsoft.App --wait
echo "==> Waiting for Microsoft.OperationalInsights..."
az provider register --namespace Microsoft.OperationalInsights --wait
echo "==> Waiting for Microsoft.Web..."
az provider register --namespace Microsoft.Web --wait
echo "==> All providers registered"

# ---------------------------------------------------------------------------
# Resource Group
# ---------------------------------------------------------------------------
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

ACR_PASSWORD=$(az acr credential show \
  --name "${ACR_NAME}" \
  --query "passwords[0].value" \
  --output tsv)

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
# Use a public placeholder image for now.
# GitHub Actions will deploy the real image on first push.
# ---------------------------------------------------------------------------
echo "==> Creating API Container App: ${API_APP_NAME}"
az containerapp create \
  --name "${API_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --environment "${CONTAINER_ENV}" \
  --image "mcr.microsoft.com/dotnet/samples:aspnetapp" \
  --target-port 5000 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 1 \
  --output none

API_URL=$(az containerapp show \
  --name "${API_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --query "properties.configuration.ingress.fqdn" \
  --output tsv)

# ---------------------------------------------------------------------------
# Static Web App (frontend)
# ---------------------------------------------------------------------------
echo "==> Creating Static Web App: ${STATIC_APP_NAME}"
az staticwebapp create \
  --name "${STATIC_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --location "eastasia" \
  --sku Free \
  --output none

STATIC_URL=$(az staticwebapp show \
  --name "${STATIC_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --query "defaultHostname" \
  --output tsv)

STATIC_TOKEN=$(az staticwebapp secrets list \
  --name "${STATIC_APP_NAME}" \
  --query "properties.apiKey" \
  --output tsv)

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
echo ""
echo "============================================================"
echo "  Infrastructure provisioned!"
echo "============================================================"
echo "  API URL (placeholder):  https://${API_URL}"
echo "  Frontend URL:           https://${STATIC_URL}"
echo ""
echo "  Add these secrets to GitHub:"
echo "  (Settings → Secrets and variables → Actions)"
echo ""
echo "  AZURE_CREDENTIALS        - see below"
echo "  ACR_LOGIN_SERVER         - ${ACR_LOGIN_SERVER}"
echo "  ACR_USERNAME             - ${ACR_NAME}"
echo "  ACR_PASSWORD             - ${ACR_PASSWORD}"
echo "  AZURE_RESOURCE_GROUP     - ${RESOURCE_GROUP}"
echo "  AZURE_CONTAINER_APP_NAME - ${API_APP_NAME}"
echo "  AZURE_STATIC_APP_TOKEN   - ${STATIC_TOKEN}"
echo "  VITE_API_URL             - https://${API_URL}"
echo ""
echo "  To generate AZURE_CREDENTIALS, run:"
echo "  az ad sp create-for-rbac \\"
echo "    --name flowingly-github-actions \\"
echo "    --role contributor \\"
echo "    --scopes /subscriptions/\$(az account show --query id -o tsv)/resourceGroups/${RESOURCE_GROUP} \\"
echo "    --sdk-auth"
echo ""
echo "  Then push to main to trigger the first real deployment."
echo "============================================================"

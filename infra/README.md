# Azure Deployment

This app deploys to:
- **Azure Container Apps** — .NET 8 API (free tier: 180k vCPU-seconds/month)
- **Azure Static Web Apps** — React frontend (free tier, CDN-backed)
- **Azure Container Registry** — Docker image storage (Basic, ~$5/month — the only paid component)

> The free Azure account includes a $200 credit which covers ACR and more for the first 30 days.
> After that, ACR Basic is ~$5/month. Everything else stays within the free tier.

---

## First-time setup

### 1. Install the Azure CLI

```bash
# macOS
brew install azure-cli

# Windows
winget install Microsoft.AzureCLI
```

### 2. Log in

```bash
az login
```

### 3. Run the provisioning script

From the repo root:

```bash
chmod +x infra/deploy.sh
./infra/deploy.sh
```

This will:
- Create a resource group, ACR, Container Apps environment, API Container App, and Static Web App
- Build and push the API Docker image
- Print the URLs and the GitHub secrets you need to add

---

## GitHub Actions CI/CD

After the first deploy, every push to `main` automatically rebuilds and redeploys both services.

### Required GitHub secrets

Add these in your repo under **Settings → Secrets and variables → Actions**:

| Secret | How to get it |
|---|---|
| `AZURE_CREDENTIALS` | See below |
| `ACR_LOGIN_SERVER` | Printed by deploy.sh, e.g. `flowinglyacr.azurecr.io` |
| `ACR_USERNAME` | Same as your ACR name, e.g. `flowinglyacr` |
| `ACR_PASSWORD` | `az acr credential show --name flowinglyacr --query "passwords[0].value" -o tsv` |
| `AZURE_RESOURCE_GROUP` | `flowingly-rg` |
| `AZURE_CONTAINER_APP_NAME` | `flowingly-api` |
| `AZURE_STATIC_APP_TOKEN` | `az staticwebapp secrets list --name flowingly-frontend --query "properties.apiKey" -o tsv` |
| `VITE_API_URL` | The API URL printed by deploy.sh, e.g. `https://flowingly-api.azurecontainerapps.io` |

### Creating the AZURE_CREDENTIALS service principal

```bash
az ad sp create-for-rbac \
  --name "flowingly-github-actions" \
  --role contributor \
  --scopes /subscriptions/<your-subscription-id>/resourceGroups/flowingly-rg \
  --sdk-auth
```

Copy the entire JSON output and paste it as the `AZURE_CREDENTIALS` secret.

To find your subscription ID:

```bash
az account show --query id -o tsv
```

---

## Architecture

```
GitHub Actions (push to main)
  ├── deploy-api job
  │     ├── docker build + push → Azure Container Registry
  │     └── az containerapp update → Azure Container Apps
  └── deploy-frontend job
        ├── npm ci + vite build (with VITE_API_URL injected)
        └── Azure Static Web Apps deploy action
```

```
Browser → Azure Static Web Apps (React)
               │
               │ fetch https://<api-url>/api/import/parse
               ▼
          Azure Container Apps (.NET 8 API)
```

---

## Local development (unchanged)

Docker Compose and the Vite dev server work exactly as before — no changes required:

```bash
# Docker Compose
docker compose up --build

# Local dev servers
cd backend/Flowingly.Import.Api && dotnet run --urls http://localhost:5000
cd frontend && npm run dev
```

`VITE_API_URL` is not set locally, so the frontend falls back to `/api` which is proxied by Vite (dev) or nginx (Docker Compose).

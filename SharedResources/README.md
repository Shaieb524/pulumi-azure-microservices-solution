# SharedResources - Azure Infrastructure Template

Pulumi C# template for deploying shared Azure resources with centralized configuration management.

## 📁 Project Structure

### Core Infrastructure Files
- **`Program.cs`** - Entry point for Pulumi
- **`pulumi.yaml`** - Project definition
- **`pulumi.demov2.yaml`** - Stack configuration (location, SKUs, resource names)
- **`SharedResources.csproj`** - C# project file with dependencies

### Configuration Management
- **`ConfigParser.cs`** - Utility for parsing YAML config to C# objects
- **`DeploymentConfigs.cs`** - Main configuration class
- **`SecretAccess.cs`** - Handles encrypted secrets

### Infrastructure Components
- **`ContainerizedStack.cs`** - Main stack definition
- **`ContainerizedStack.SqlServer.cs`** - SQL Server creation
- **`ContainerizedStack.Databases.cs`** - SQL Database creation
- **`ContainerizedStack.StorageAccount.cs`** - Storage Account creation
- **`ContainerizedStack.ContainerRegistry.cs`** - Azure Container Registry
- **`ContainerizedStack.ApiManagement.cs`** - API Management service

### Setup Scripts
- **`setup-esc.ps1`** - Creates Pulumi ESC environment for shared config
- **`add-secrets.ps1`** - Sets encrypted secrets (SQL passwords, etc.)

### Post-Deployment Scripts
- **`update-esc.ps1`** - Adds Docker Registry credentials to ESC
- **`update-sql-esc.ps1`** - Adds SQL connection strings to ESC
- **`update-apim-esc.ps1`** - Adds APIM URLs to ESC
- **`import-apis-to-apim.ps1`** - Imports APIs from Swagger URLs to APIM

### Test Scripts
- **`test-*.ps1`** - Scripts to verify deployed resources

## 🚀 Getting Started

### 1. Initial Setup
```powershell
# Create ESC environment for shared configuration
.\setup-esc.ps1

# Set secrets (update the password first)
.\add-secrets.ps1
```

### 2. Deploy Infrastructure
```powershell
# Install dependencies and deploy
dotnet restore
pulumi up
```

### 3. Post-Deployment Configuration
```powershell
# Add Docker Registry credentials to ESC
.\update-esc.ps1

# Add SQL connection strings to ESC  
.\update-sql-esc.ps1

# Add APIM URLs to ESC
.\update-apim-esc.ps1

# Import your APIs to APIM (update swagger URLs first)
.\import-apis-to-apim.ps1
```

### 4. Test Deployment
```powershell
# Test individual services
.\test-containerregistry.ps1 -RegistryName "proj1registry" -ResourceGroupName "client1-proj1-SharedResources-RG"
.\test-apimanagement.ps1 -ApiManagementName "proj1-apim" -ResourceGroupName "client1-proj1-SharedResources-RG"
```

## 📦 What Gets Deployed

### Azure Resources
- **Resource Group** - Container for all resources
- **SQL Server** - Database server with 10 databases
- **Storage Account** - Blob storage
- **Container Registry** - Docker image registry
- **API Management** - API gateway with Application Insights

### ESC Configuration
Shared configuration available to all projects:
- Basic settings (Location, Prefix, Tags)
- Docker Registry credentials
- SQL connection strings for all databases
- APIM URLs

## 🔗 Using in Other Projects

Add to any project's `pulumi.yaml`:
```yaml
environment:
  - AHOY2025-org/default/shared-config
```

Then access shared config:
```csharp
var config = new Config("SharedResources");
var registryUrl = config.Require("DockerSettings:DockerRegistryUrl");
var sqlConnection = config.RequireSecret("ConnectionStrings:App1Db");
var apimGateway = config.Require("ApiManagement:GatewayUrl");
```

## ⚙️ Customization

- **Update resource names** in `pulumi.demov2.yaml`
- **Add new APIs** to `import-apis-to-apim.ps1`
- **Modify SKUs/sizes** in configuration files
- **Add new databases** to the `Databases` array

## 📊 Outputs

After deployment, get outputs with:
```powershell
pulumi stack output  # See all outputs
pulumi stack output SqlServerFqdn
pulumi stack output ContainerRegistryLoginServer
pulumi stack output ApiManagementGatewayUrl
```
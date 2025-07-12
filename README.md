# Azure Microservices Infrastructure Template

This repository provides a clean, reusable template for deploying Azure microservices infrastructure using Pulumi and C#. The template follows modern cloud architecture patterns with shared resources and modular service configurations.

## Architecture Overview

The solution consists of three main components:

- **SharedResources**: Common infrastructure components (SQL Server, API Management, Container Registry, Storage)
- **App1**: Template microservice with App Service, Function App, Storage, and Event Grid integration
- **Bounty**: Example microservice implementation demonstrating the template patterns

## Project Structure

```
BackendInfra.sln
├── SharedResources/              # Shared infrastructure components
│   ├── stack/                    # Infrastructure stack definitions
│   │   ├── ContainerizedStack.cs
│   │   ├── ContainerizedStack.ApiManagement.cs
│   │   ├── ContainerizedStack.ContainerRegistry.cs
│   │   ├── ContainerizedStack.Databases.cs
│   │   ├── ContainerizedStack.SqlServer.cs
│   │   └── ContainerizedStack.StorageAccount.cs
│   ├── helpers/                  # Configuration and utility classes
│   │   ├── ConfigParser.cs
│   │   ├── DeploymentConfigs.cs
│   │   └── SecretAccess.cs
│   ├── pre-deploy-scripts/       # Pre-deployment automation
│   └── post-deploy-scripts/      # Post-deployment automation
├── App1/                         # Template microservice
│   ├── stack/                    # Service-specific infrastructure
│   │   ├── App1Stack.cs
│   │   ├── App1Stack.App1ApiAppService.cs
│   │   ├── App1Stack.App1FunctionApp.cs
│   │   ├── App1Stack.App1StorageAccount.cs
│   │   └── App1Stack.App1EventGrid.cs
│   └── helpers/                  # Service configuration helpers
│       ├── App1DeploymentConfigs.cs
│       └── App1SecretAccess.cs
└── Bounty/                       # Example implementation
    ├── stack/                    # Bounty-specific infrastructure
    ├── helpers/                  # Bounty configuration helpers
    ├── pre-deploy-scripts/       # Bounty pre-deployment scripts
    └── post-deploy-scripts/      # Bounty post-deployment scripts
```

## Features

### Shared Infrastructure
- **API Management**: Centralized API gateway with policies and security
- **Container Registry**: Docker image hosting for microservices
- **SQL Server**: Shared database server with individual service databases
- **Storage Accounts**: Centralized blob storage and table storage
- **Application Insights**: Unified monitoring and logging

### Microservice Template (App1)
- **App Service**: Containerized web API with auto-scaling
- **Function App**: Event-driven serverless functions
- **Storage Account**: Service-specific blob and table storage
- **Event Grid**: Custom event topics for service communication
- **Application Insights**: Service-specific monitoring

### Configuration Management
- **Environment-based configs**: Separate YAML files for each environment
- **Secret management**: Secure handling of sensitive configuration
- **Shared resource integration**: Automatic linking to shared components

## Getting Started

### Prerequisites
- [Pulumi CLI](https://www.pulumi.com/docs/get-started/install/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Azure subscription with appropriate permissions

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd pulumi-azure-microservices-solution
   ```

2. **Configure Azure authentication**
   ```bash
   az login
   pulumi config set azure-native:location <your-region>
   ```

3. **Deploy shared resources first**
   ```bash
   cd SharedResources
   pulumi stack init <environment-name>
   pulumi up
   ```

4. **Deploy microservices**
   ```bash
   cd ../App1
   pulumi stack init <environment-name>
   pulumi up
   ```

### Creating a New Microservice

1. **Copy the App1 template**
   ```bash
   cp -r App1 YourServiceName
   ```

2. **Update project files**
   - Rename `App1.csproj` to `YourServiceName.csproj`
   - Update namespace references from `App1` to `YourServiceName`
   - Modify `pulumi.yaml` project name

3. **Configure service-specific settings**
   - Update `helpers/YourServiceNameDeploymentConfigs.cs`
   - Modify `helpers/YourServiceNameSecretAccess.cs`
   - Customize stack components in `stack/` folder

4. **Deploy your new service**
   ```bash
   cd YourServiceName
   pulumi stack init <environment-name>
   pulumi up
   ```

## Configuration

### Environment Files
Each service includes environment-specific YAML files:
- `pulumi.<environment-name>.yaml`: Environment-specific configuration
- `Pulumi.yaml`: Project definition and runtime settings

### Secrets Management
Sensitive configuration is handled through:
- Azure Key Vault integration
- Pulumi secret configuration
- Environment-specific secret files

### Resource Naming
Resources follow a consistent naming pattern:
- Format: `{prefix}-{service}-{resource}-{environment}-{suffix}`
- Example: `myapp-app1-webapp-dev-001`

## Deployment Scripts

### Pre-deployment
- **setup-esc.ps1**: Configure Pulumi ESC (Environment, Secrets, Configuration)
- **add-secrets.ps1**: Add required secrets to configuration

### Post-deployment
- **update-app-settings.ps1**: Configure application settings
- **import-apis-to-apim.ps1**: Register APIs with API Management
- **move-acr-repos.ps1**: Organize container registry repositories

## Best Practices

1. **Resource Organization**: Use resource groups to logically separate environments
2. **Naming Conventions**: Follow consistent naming patterns across all resources
3. **Security**: Use managed identities and Key Vault for secrets
4. **Monitoring**: Configure Application Insights for all services
5. **Scaling**: Design for horizontal scaling with stateless services
6. **Cost Management**: Use appropriate SKUs and implement auto-scaling

## Support

For questions and issues:
1. Check the documentation in each project folder
2. Review the example implementation in the Bounty project
3. Consult Pulumi and Azure documentation for specific resource configuration

## License

This template is provided under the MIT License. See LICENSE file for details.
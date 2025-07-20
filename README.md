https://wellrounded.engineer/azure-infrastructure-templates-that-work-with-ai-my-experience-building-a-reusable-pulumi-solution/

# Azure Microservices Infrastructure Template

This repository provides a clean, reusable template for deploying Azure microservices infrastructure using Pulumi and C#. The template follows modern cloud architecture patterns with shared resources and modular service configurations.

## Architecture Overview

The solution consists of three main components:

- **SharedResources**: Common infrastructure components (SQL Server, API Management, Container Registry)
- **App1**: Clean template microservice with App Service, Function App, Storage, and Event Grid integration
- **App2**: Second template microservice demonstrating the reusable template patterns

## Project Structure

```
BackendInfra.sln
├── SharedResources/              # Shared infrastructure components
│   ├── stack/                    # Infrastructure stack definitions
│   │   ├── Stack.cs
│   │   ├── Stack.ApiManagement.cs
│   │   ├── Stack.ContainerRegistry.cs
│   │   ├── Stack.Databases.cs
│   │   └── Stack.SqlServer.cs
│   ├── helpers/                  # Configuration and utility classes
│   │   ├── ConfigParser.cs
│   │   ├── DeploymentConfigs.cs
│   │   └── SecretAccess.cs
│   ├── pre-deploy-scripts/       # Pre-deployment automation
│   │   ├── setup-pulumi-esc.ps1
│   │   └── add-secrets.ps1
│   └── post-deploy-scripts/      # Post-deployment automation
│
├── App1/                         # Template microservice (clean template)
│   ├── stack/                    # Service-specific infrastructure
│   │   ├── App1Stack.cs
│   │   ├── App1Stack.App1ApiAppService.cs
│   │   ├── App1Stack.App1FunctionApp.cs
│   │   ├── App1Stack.App1StorageAccount.cs
│   │   └── App1Stack.App1EventsEventGrid.cs
│   ├── helpers/                  # Service configuration helpers
│   │   ├── App1DeploymentConfigs.cs
│   │   └── App1SecretAccess.cs
│   └── pre-deploy-scripts/       # Service setup scripts
│       └── add-secrets.ps1
│
└── App2/                         # Second template microservice
    ├── stack/                    # Service-specific infrastructure
    │   ├── App2Stack.cs
    │   ├── App2Stack.App2ApiAppService.cs
    │   ├── App2Stack.App2FunctionApp.cs
    │   ├── App2Stack.App2StorageAccount.cs
    │   └── App2Stack.App2EventsEventGrid.cs
    ├── helpers/                  # Service configuration helpers
    │   ├── App2DeploymentConfigs.cs
    │   └── App2SecretAccess.cs
    └── pre-deploy-scripts/       # Service setup scripts
        └── add-secrets.ps1
```

## Features

### Shared Infrastructure
- **API Management**: Centralized API gateway with policies and security
- **Container Registry**: Docker image hosting for microservices
- **SQL Server**: Shared database server with individual service databases (App1Db, App2Db by default)
- **Application Insights**: Unified monitoring and logging

### Microservice Templates (App1 & App2)
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
   
   # Deploy App2 (optional - second template example)
   cd ../App2
   pulumi stack init <environment-name>
   pulumi up
   ```

### Creating a New Microservice

Both App1 and App2 serve as clean templates. Choose either one as your starting point:

1. **Copy a template**
   ```bash
   # Use App1 template
   cp -r App1 YourServiceName
   
   # Or use App2 template
   cp -r App2 YourServiceName
   ```

2. **Update project files**
   - Rename `App1.csproj` (or `App2.csproj`) to `YourServiceName.csproj`
   - Update namespace references from `App1` (or `App2`) to `YourServiceName`
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
- `pulumi.client1-dev.yaml`: Environment-specific configuration
- `Pulumi.yaml`: Project definition and runtime settings

### Secrets Management
Sensitive configuration is handled through:
- Pulumi ESC (Environment, Secrets, Configuration)
- Pulumi secret configuration
- Environment-specific secret files

### Resource Naming
Resources follow a consistent naming pattern:
- Format: `{client}-{prefix}-{resource}-{environment}`
- Example: `client1-dev-app1-webapp`

## Deployment Scripts

### Pre-deployment
- **setup-pulumi-esc.ps1**: Configure Pulumi ESC (Environment, Secrets, Configuration)
- **add-client1-secrets.ps1**: Add required secrets to configuration

### Post-deployment
- **update-app-settings.ps1**: Configure application settings
- **update-function-settings.ps1**: Configure function app settings

## Best Practices

1. **Resource Organization**: Use resource groups to logically separate environments
2. **Naming Conventions**: Follow consistent naming patterns across all resources
3. **Security**: Use managed identities and Key Vault for secrets
4. **Monitoring**: Configure Application Insights for all services
5. **Scaling**: Design for horizontal scaling with stateless services
6. **Cost Management**: Use appropriate SKUs and implement auto-scaling

## Support

For questions and issues:
1. Check the documentation in each project folder (README.md files)
2. Review the clean template implementations in App1 and App2
3. Consult Pulumi and Azure documentation for specific resource configuration

## Template Comparison

- **App1**: Clean template with minimal configuration
- **App2**: Second clean template demonstrating reusability  

These templates provide flexible starting points for new microservices with consistent patterns and best practices.

## License

This template is provided under the MIT License. See LICENSE file for details.

More details here:
https://wellrounded.engineer/azure-infrastructure-templates-that-work-with-ai-my-experience-building-a-reusable-pulumi-solution/

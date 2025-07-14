# App2 Infrastructure Template

This is a clean template for creating Azure infrastructure using Pulumi, following a microservices pattern with App Services, Function Apps, Storage Accounts, and Event Grid.

## Directory Structure

```
App2/
├── helpers/
│   ├── App2DeploymentConfigs.cs
│   └── App2SecretAccess.cs
├── stack/
│   ├── App2Stack.cs
│   ├── App2Stack.App2ApiAppService.cs
│   ├── App2Stack.App2FunctionApp.cs
│   ├── App2Stack.App2StorageAccount.cs
│   └── App2Stack.App2EventsEventGrid.cs
├── pre-deploy-scripts/
│   ├── README.md
│   └── add-secrets.ps1
├── App2.csproj
├── Program.cs
├── pulumi.client1-dev.yaml
└── pulumi.yaml
```

## Key Components

### 1. Helpers

- **App2DeploymentConfigs.cs**: Configuration management with SharedResources integration
- **App2SecretAccess.cs**: Secure secret handling and blob connection string management

### 2. Stack Components

- **App2Stack.cs**: Main orchestration stack
- **App2Stack.App2ApiAppService.cs**: API App Service with comprehensive configuration
- **App2Stack.App2FunctionApp.cs**: Function App with proper settings
- **App2Stack.App2StorageAccount.cs**: Storage Account configuration
- **App2Stack.App2EventsEventGrid.cs**: Event Grid Topic for event-driven architecture

### 3. Configuration Structure

```yaml
# Docker Settings
App2Infrastructure:DockerSettings:
  DockerApp2ApiImageName: app2-api
  DockerApp2ApiImageTag: latest-20250404133552
  DockerApp2FnImageName: app2-fn
  DockerApp2FnImageTag: latest-20250404133552

# Resource Names
App2Infrastructure:ResourcesNames:
  ResourceGroupName: app2-rg
  App2ApiAppServicePlanName: app2-apis-plan
  App2ApiAppServiceName: app2-apis
  App2ApiAppInsightsName: app2-insights
  App2FnAppPlanName: app2-functions-plan
  App2FnAppName: app2-functions
  App2FnAppInsightsName: app2-functions-insights
  App2FnStorageAccountName: app2fnsa
  App2StorageAccountName: app2sa
  App2EventsBookingTopicName: app2-someevent
```

## Deployment

### Prerequisites
1. Ensure SharedResources is deployed first
2. Configure secrets using the pre-deploy script

### Steps
1. **Set up secrets**:
   ```bash
   cd pre-deploy-scripts
   .\add-secrets.ps1
   ```

2. **Deploy infrastructure**:
   ```bash
   dotnet restore
   pulumi up
   ```

## Resources Created

- **Resource Group**: Container for all App2 resources
- **App Service**: Containerized API with Application Insights
- **Function App**: Serverless functions with dedicated storage
- **Storage Account**: Blob storage for App2 data
- **Event Grid Topic**: Event-driven communication
- **Application Insights**: Monitoring for both API and Functions

## Template Features

- **Clean Architecture**: No domain-specific dependencies
- **Shared Resources**: Integrates with SharedResources for common configurations
- **Event-Driven**: Event Grid integration for microservices communication
- **Containerized**: Docker support for both API and Function Apps
- **Secure**: Proper secret management with encrypted storage
- **Scalable**: Modern Azure PaaS services with auto-scaling capabilities
- **Monitoring**: Application Insights integration for observability

## Customization

To use this template for your project:

1. Replace `App2` with your service name throughout the codebase
2. Update the `pulumi.client1-dev.yaml` with your specific configuration
3. Modify the application settings to match your requirements
4. Add/remove Azure resources as needed
5. Update the secret management to match your security requirements

This template provides a solid foundation for Azure microservices infrastructure while remaining flexible and extensible.

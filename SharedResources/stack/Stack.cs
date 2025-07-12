using System;
using System.Linq;
using Pulumi;
using Pulumi.AzureNative.Resources;

namespace SharedResources
{
    public partial class Stack : Pulumi.Stack
    {
        public Stack()
        {
            var config = new Config("SharedResources");
            var deploymentConfigs = new DeploymentConfigs(config);

            // 1. Resource Group
            var resourceGroup = new ResourceGroup(deploymentConfigs.ResourcesNames["ResourceGroupName"], new ResourceGroupArgs
            {
                ResourceGroupName = $"{deploymentConfigs.Client}-{deploymentConfigs.Prefix}-{deploymentConfigs.ResourcesNames["ResourceGroupName"]}",
                Location = deploymentConfigs.Location,
                Tags = deploymentConfigs.CommonTags
            });

            // 2. SQL Database Server
            var sqlServer = CreateSqlServer(deploymentConfigs, resourceGroup);

            // 4. Container Registry
            var containerRegistry = CreateContainerRegistry(deploymentConfigs, resourceGroup);

            // 5. Container Registry Credentials
            var (registryUsername, registryPassword) = GetContainerRegistryCredentials(resourceGroup, containerRegistry);

            // 6. API Management with Application Insights
            var (apiManagement, apiManagementInsights) = CreateApiManagement(deploymentConfigs, resourceGroup);

            // 7. SQL Databases
            var databases = CreateDatabases(deploymentConfigs, resourceGroup, sqlServer);

            // Outputs
            this.ResourceGroupName = resourceGroup.Name;
            this.SqlServerName = sqlServer.Name;
            this.SqlServerFqdn = sqlServer.FullyQualifiedDomainName;
            this.ContainerRegistryName = containerRegistry.Name;
            this.ContainerRegistryLoginServer = containerRegistry.LoginServer;
            this.ContainerRegistryUsername = registryUsername;
            this.ContainerRegistryPassword = registryPassword;
            this.ApiManagementName = apiManagement.Name;
            this.ApiManagementGatewayUrl = apiManagement.GatewayUrl;
            this.ApiManagementPortalUrl = apiManagement.PortalUrl;
            this.ApiManagementInsightsName = apiManagementInsights.Name;
            this.ApiManagementInsightsInstrumentationKey = apiManagementInsights.InstrumentationKey;
            this.ApiManagementInsightsConnectionString = apiManagementInsights.ConnectionString;
            this.DatabaseNames = Output.All(databases.Select(db => db.Name)).Apply(names => names.ToArray());
        }

        [Output] public Output<string> ResourceGroupName { get; set; }
        [Output] public Output<string> SqlServerName { get; set; }
        [Output] public Output<string> SqlServerFqdn { get; set; }
        [Output] public Output<string> ContainerRegistryName { get; set; }
        [Output] public Output<string> ContainerRegistryLoginServer { get; set; }
        [Output] public Output<string> ContainerRegistryUsername { get; set; }
        [Output] public Output<string> ContainerRegistryPassword { get; set; }
        [Output] public Output<string> ApiManagementName { get; set; }
        [Output] public Output<string> ApiManagementGatewayUrl { get; set; }
        [Output] public Output<string> ApiManagementPortalUrl { get; set; }
        [Output] public Output<string> ApiManagementInsightsName { get; set; }
        [Output] public Output<string> ApiManagementInsightsInstrumentationKey { get; set; }
        [Output] public Output<string> ApiManagementInsightsConnectionString { get; set; }
        [Output] public Output<string[]> DatabaseNames { get; set; }
    }
}
using System;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Sql;
using SharedResources.helpers;

namespace SharedResources
{
    public partial class Stack
    {
        #region SQL Database Server
        private static Server CreateSqlServer(DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            var sqlServer = new Server(deploymentConfigs.ResourcesNames["DatabaseServerName"], new ServerArgs
            {

                ServerName = $"{deploymentConfigs.Client}-{deploymentConfigs.Prefix}-{deploymentConfigs.ResourcesNames["DatabaseServerName"]}",
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Tags = deploymentConfigs.CommonTags,
                AdministratorLogin = "sqladmin",
                AdministratorLoginPassword = deploymentConfigs.Secrets.SqlAdminPassword,
                Version = "12.0",
                MinimalTlsVersion = "1.2"
            });

            // Create firewall rule to allow Azure services
            var firewallRule = new FirewallRule("AllowAzureServices", new FirewallRuleArgs
            {
                FirewallRuleName = "AllowAzureServices",
                ResourceGroupName = resourceGroup.Name,
                ServerName = sqlServer.Name,
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0"
            });

            return sqlServer;
        }
        #endregion
    }
}
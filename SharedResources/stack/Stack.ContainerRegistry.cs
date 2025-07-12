using System;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.ContainerRegistry.Inputs;
using SharedResources.helpers;

namespace SharedResources
{
    public partial class Stack
    {
        #region Container Registry
        private static Registry CreateContainerRegistry(DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            var registryConfig = deploymentConfigs.ContainerRegistry;

            var containerRegistry = new Registry(deploymentConfigs.ResourcesNames["ContainerRegistryName"], new RegistryArgs
            {
                RegistryName = $"{deploymentConfigs.Client.ToLower()}{deploymentConfigs.Prefix.ToLower()}{deploymentConfigs.ResourcesNames["ContainerRegistryName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                Sku = new SkuArgs
                {
                    Name = ParseRegistrySkuName(registryConfig["SkuName"].ToString()!)
                },
                AdminUserEnabled = bool.Parse(registryConfig["AdminUserEnabled"].ToString()!),
            });

            return containerRegistry;
        }

        private static SkuName ParseRegistrySkuName(string skuName)
        {
            return skuName.ToLower() switch
            {
                "basic" => SkuName.Basic,
                "standard" => SkuName.Standard,
                "premium" => SkuName.Premium,
                _ => SkuName.Basic
            };
        }

        private static (Output<string> username, Output<string> password) GetContainerRegistryCredentials(ResourceGroup resourceGroup, Registry containerRegistry)
        {
            var credentials = ListRegistryCredentials.Invoke(new ListRegistryCredentialsInvokeArgs
            {
                ResourceGroupName = resourceGroup.Name,
                RegistryName = containerRegistry.Name
            });

            var username = credentials.Apply(creds => creds.Username ?? "");
            var password = credentials.Apply(creds =>
                creds.Passwords.Length > 0 ? creds.Passwords[0].Value ?? "" : "");

            return (username, password);
        }

        #endregion
    }
}
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using App1.helpers;

namespace App1.stack
{
    public partial class App1Stack
    {
        private static StorageAccount CreateApp1StorageAccount(App1DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            var storageAccount = new StorageAccount(deploymentConfigs.ResourcesNames["App1StorageAccountName"], new StorageAccountArgs
            {
                AccountName = $"{deploymentConfigs.Client.ToLower()}{deploymentConfigs.Prefix.ToLower()}{deploymentConfigs.ResourcesNames["App1StorageAccountName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                Kind = Enum.Parse<Kind>(deploymentConfigs.StorageAccount["Kind"].ToString()!),
                Sku = new SkuArgs
                {
                    Name = Enum.Parse<SkuName>(deploymentConfigs.StorageAccount["SkuName"].ToString()!)
                },
                AccessTier = Enum.Parse<AccessTier>(deploymentConfigs.StorageAccount["AccessTier"].ToString()!),
                AllowBlobPublicAccess = bool.Parse(deploymentConfigs.StorageAccount["AllowBlobPublicAccess"].ToString()!),
                MinimumTlsVersion = Enum.Parse<MinimumTlsVersion>(deploymentConfigs.StorageAccount["MinimumTlsVersion"].ToString()!),
                EnableHttpsTrafficOnly = bool.Parse(deploymentConfigs.StorageAccount["EnableHttpsTrafficOnly"].ToString()!)
            });

            return storageAccount;
        }
    }
}

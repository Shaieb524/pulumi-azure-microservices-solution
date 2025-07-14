using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using App1.helpers;
using System;

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
                Kind = ParseKind(deploymentConfigs.StorageAccount["Kind"].ToString()!),
                Sku = new SkuArgs
                {
                    Name = ParseSkuName(deploymentConfigs.StorageAccount["SkuName"].ToString()!)
                },
                AccessTier = ParseAccessTier(deploymentConfigs.StorageAccount["AccessTier"].ToString()!),
                AllowBlobPublicAccess = bool.Parse(deploymentConfigs.StorageAccount["AllowBlobPublicAccess"].ToString()!),
                MinimumTlsVersion = ParseMinimumTlsVersion(deploymentConfigs.StorageAccount["MinimumTlsVersion"].ToString()!),
                EnableHttpsTrafficOnly = bool.Parse(deploymentConfigs.StorageAccount["EnableHttpsTrafficOnly"].ToString()!)
            });

            return storageAccount;
        }

        // Helper methods to parse the enum values safely
        private static Kind ParseKind(string value)
        {
            return value switch
            {
                "StorageV2" => Kind.StorageV2,
                "Storage" => Kind.Storage,
                "BlobStorage" => Kind.BlobStorage,
                "BlockBlobStorage" => Kind.BlockBlobStorage,
                "FileStorage" => Kind.FileStorage,
                _ => Kind.StorageV2 // Default value
            };
        }

        private static SkuName ParseSkuName(string value)
        {
            return value switch
            {
                "Standard_LRS" => SkuName.Standard_LRS,
                "Standard_GRS" => SkuName.Standard_GRS,
                "Standard_RAGRS" => SkuName.Standard_RAGRS,
                "Standard_ZRS" => SkuName.Standard_ZRS,
                "Premium_LRS" => SkuName.Premium_LRS,
                "Premium_ZRS" => SkuName.Premium_ZRS,
                "Standard_GZRS" => SkuName.Standard_GZRS,
                "Standard_RAGZRS" => SkuName.Standard_RAGZRS,
                _ => SkuName.Standard_LRS // Default value
            };
        }

        private static AccessTier ParseAccessTier(string value)
        {
            return value switch
            {
                "Hot" => AccessTier.Hot,
                "Cool" => AccessTier.Cool,
                _ => AccessTier.Hot // Default value
            };
        }

        private static MinimumTlsVersion ParseMinimumTlsVersion(string value)
        {
            return value switch
            {
                "TLS1_0" => MinimumTlsVersion.TLS1_0,
                "TLS1_1" => MinimumTlsVersion.TLS1_1,
                "TLS1_2" => MinimumTlsVersion.TLS1_2,
                _ => MinimumTlsVersion.TLS1_2 // Default value
            };
        }
    }
}

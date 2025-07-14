using System;
using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using App1.helpers;
using static Pulumi.AzureNative.Storage.ListStorageAccountKeys;

namespace App1.stack
{
    public partial class App1Stack
    {
        private static WebApp CreateApp1FunctionApp(App1DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            // Storage Account for Functions
            var functionStorageAccount = new StorageAccount(deploymentConfigs.ResourcesNames["App1FnStorageAccountName"], new StorageAccountArgs
            {
                AccountName = $"{deploymentConfigs.Client.ToLower()}{deploymentConfigs.Prefix.ToLower()}{deploymentConfigs.ResourcesNames["App1FnStorageAccountName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
                Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
                {
                    Name = SkuName.Standard_LRS
                }
            });

            // Function App Service Plan (Consumption)
            var appServicePlan = new AppServicePlan(deploymentConfigs.ResourcesNames["App1FnAppPlanName"], new AppServicePlanArgs
            {
                Name = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1FnAppPlanName"]}",
                Location = resourceGroup.Location,
                Kind = "linux",
                ResourceGroupName = resourceGroup.Name,
                Tags = deploymentConfigs.CommonTags,
                Reserved = true,
                Sku = new SkuDescriptionArgs
                {
                    Name = deploymentConfigs.PlanSku["Name"].ToString()!,
                    Tier = deploymentConfigs.PlanSku["Tier"].ToString()!,
                    Size = deploymentConfigs.PlanSku["Size"].ToString()!,
                    Family = deploymentConfigs.PlanSku["Family"].ToString()!,
                    Capacity = Convert.ToInt32(deploymentConfigs.PlanSku["Capacity"].ToString())
                }
            });

            // Application Insights
            var appInsights = new Component(deploymentConfigs.ResourcesNames["App1FnAppInsightsName"], new ComponentArgs
            {
                ResourceName = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1FnAppInsightsName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                ApplicationType = ApplicationType.Web,
                Kind = "web"
            });

            // Function App
            var functionApp = new WebApp(deploymentConfigs.ResourcesNames["App1FnAppName"], new WebAppArgs
            {
                Name = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1FnAppName"]}",
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Kind = "functionapp,linux,container",
                ServerFarmId = appServicePlan.Id,
                SiteConfig = new SiteConfigArgs
                {
                    LinuxFxVersion = deploymentConfigs.App1FnLinuxFxVersion,
                    AppSettings = GetApp1FunctionAppSettings(deploymentConfigs, appInsights, resourceGroup.Name, functionStorageAccount.Name),
                    HealthCheckPath = deploymentConfigs.App1FnAppSettings["HealthCheck"].ToString() ?? "/api/HealthCheck",
                },
                Tags = deploymentConfigs.CommonTags
            });

            return functionApp;
        }

        private static NameValuePairArgs[] GetApp1FunctionAppSettings(App1DeploymentConfigs deploymentConfigs, Component appInsights, Output<string> resourceGroupName, Output<string> storageAccountName)
        {
            var storageConnectionString = CreateStorageConnectionString(resourceGroupName, storageAccountName);
            var secrets = deploymentConfigs.Secrets;

            var settings = new List<NameValuePairArgs>
            {
                // Core Function settings
                new() { Name = "AzureWebJobsStorage", Value = storageConnectionString },
                new() { Name = "FUNCTIONS_WORKER_RUNTIME", Value = "dotnet" },
                new() { Name = "FUNCTIONS_EXTENSION_VERSION", Value = "~4" },
                new() { Name = "WEBSITE_RUN_FROM_PACKAGE", Value = "0" },
                new() { Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE", Value = "false" },
                new() { Name = "AzureWebJobsSecretStorageType", Value = "files" },
                // Application Insights
                new() { Name = "APPLICATIONINSIGHTS_CONNECTION_STRING", Value = appInsights.ConnectionString },
                
                // Docker settings
                new() { Name = "DOCKER_REGISTRY_SERVER_URL", Value = deploymentConfigs.DockerRegistryUrl },
                new() { Name = "DOCKER_REGISTRY_SERVER_USERNAME", Value = deploymentConfigs.DockerRegistryUserName },
                new() { Name = "DOCKER_REGISTRY_SERVER_PASSWORD", Value = deploymentConfigs.DockerRegistryPassword }
            };

            AddFunctionSettings(settings, deploymentConfigs);

            return settings.ToArray();
        }

        private static void AddFunctionSettings(List<NameValuePairArgs> settings, App1DeploymentConfigs deploymentConfigs)
        {
            var fnSettings = deploymentConfigs.App1FnAppSettings;
            var secrets = deploymentConfigs.Secrets;

            if (fnSettings.TryGetValue("Values", out var valuesObj) && valuesObj is Dictionary<string, object> valuesDict)
            {
                if (valuesDict.TryGetValue("FUNCTIONS_WORKER_RUNTIME", out var runtime))
                    settings.Add(new() { Name = "FUNCTIONS_WORKER_RUNTIME", Value = runtime.ToString()! });
            }

            // Common Settings
            if (fnSettings.TryGetValue("Common", out var commonObj) && commonObj is Dictionary<string, object> commonDict)
            {
                foreach (var kvp in commonDict)
                {
                    settings.Add(new() { Name = $"Common__{kvp.Key}", Value = kvp.Value.ToString()! });
                }
            }

            // Secure values using secrets
            settings.Add(new() { Name = "ApiKey", Value = secrets.ApiKey });
            settings.Add(new() { Name = "PrimaryBlobAccountKey", Value = secrets.PrimaryBlobAccountKey });
            settings.Add(new() { Name = "SecondaryBlobAccountKey", Value = secrets.SecondaryBlobAccountKey });
            settings.Add(new() { Name = "App1BlobAccountKey", Value = secrets.App1BlobAccountKey });
            settings.Add(new() { Name = "App1EventsTopicKey", Value = secrets.App1EventsTopicKey });
            settings.Add(new() { Name = "App1FnStorageKey", Value = secrets.App1FnStorageKey });
            
            // Connection strings
            settings.Add(new() { Name = "PrimaryDataConnection", Value = secrets.PrimaryDbConnection });
            settings.Add(new() { Name = "SecondaryDataConnection", Value = secrets.SecondaryDbConnection });
        }

        private static Output<string> CreateStorageConnectionString(Output<string> resourceGroupName, Output<string> storageAccountName)
        {
            var storageAccountKeys = Output.Tuple(resourceGroupName, storageAccountName).Apply(t =>
                ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
                {
                    ResourceGroupName = t.Item1,
                    AccountName = t.Item2
                }));

            return Output.Tuple(storageAccountName, storageAccountKeys).Apply(t =>
                $"DefaultEndpointsProtocol=https;AccountName={t.Item1};AccountKey={t.Item2.Keys[0].Value};EndpointSuffix=core.windows.net");
        }
    }
}

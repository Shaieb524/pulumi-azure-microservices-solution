using System;
using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using App1.helpers;

namespace App1.stack
{
    public partial class App1Stack
    {
        private static WebApp CreateApp1ApiAppService(App1DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            var appServicePlan = new AppServicePlan($"{deploymentConfigs.ResourcesNames["App1ApiAppServicePlanName"]}", new AppServicePlanArgs
            {
                Name = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1ApiAppServicePlanName"]}",
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
                    Capacity = int.Parse(deploymentConfigs.PlanSku["Capacity"].ToString()!)
                }
            });

            var appInsights = new Component($"{deploymentConfigs.ResourcesNames["App1ApiAppInsightsName"]}", new ComponentArgs
            {
                ResourceName = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1ApiAppInsightsName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                ApplicationType = ApplicationType.Web,
                Kind = "web"
            });

            var appService = new WebApp($"{deploymentConfigs.ResourcesNames["App1ApiAppServiceName"]}", new WebAppArgs
            {
                Name = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1ApiAppServiceName"]}",
                Location = resourceGroup.Location,
                ResourceGroupName = resourceGroup.Name,
                Kind = "app,linux,container",
                ServerFarmId = appServicePlan.Id,
                SiteConfig = new SiteConfigArgs
                {
                    AlwaysOn = true,
                    LinuxFxVersion = deploymentConfigs.App1ApiLinuxFxVersion,
                    AppSettings = GetApp1ApiAppSettings(deploymentConfigs, appInsights),
                    ConnectionStrings = GetApp1ApiConnStrings(deploymentConfigs),
                    HealthCheckPath = deploymentConfigs.App1ApiAppSettings["HealthCheck"].ToString() ?? "/api/health",
                },
                Tags = deploymentConfigs.CommonTags
            });

            return appService;
        }

        private static NameValuePairArgs[] GetApp1ApiAppSettings(App1DeploymentConfigs deploymentConfigs, Component appInsights)
        {
            var settings = new List<NameValuePairArgs>
            {
                // Core settings
                new() { Name = "WEBSITE_RUN_FROM_PACKAGE", Value = "0" },
                new() { Name = "APPLICATIONINSIGHTS_CONNECTION_STRING", Value = appInsights.ConnectionString },
                
                // Docker settings
                new() { Name = "DOCKER_REGISTRY_SERVER_URL", Value = deploymentConfigs.DockerRegistryUrl },
                new() { Name = "DOCKER_REGISTRY_SERVER_USERNAME", Value = deploymentConfigs.DockerRegistryUserName },
                new() { Name = "DOCKER_REGISTRY_SERVER_PASSWORD", Value = deploymentConfigs.DockerRegistryPassword }
            };

            AddApiSettings(settings, deploymentConfigs);

            return settings.ToArray();
        }

        private static void AddApiSettings(List<NameValuePairArgs> settings, App1DeploymentConfigs deploymentConfigs)
        {
            var apiSettings = deploymentConfigs.App1ApiAppSettings;
            var secrets = deploymentConfigs.Secrets;

            if (apiSettings.TryGetValue("DisableHttpsRedirection", out var disableHttps))
                settings.Add(new() { Name = "DisableHttpsRedirection", Value = disableHttps.ToString()! });

            if (apiSettings.TryGetValue("AllowedHosts", out var allowedHosts))
                settings.Add(new() { Name = "AllowedHosts", Value = allowedHosts.ToString()! });

            // App1 Events
            if (apiSettings.TryGetValue("App1Events", out var app1EventsObj) && app1EventsObj is Dictionary<string, object> app1EventsDict)
            {
                if (app1EventsDict.TryGetValue("topicEndpoint", out var topicEndpoint))
                    settings.Add(new() { Name = "App1Events__topicEndpoint", Value = topicEndpoint.ToString()! });
            }

            // Storage Settings
            if (apiSettings.TryGetValue("StorageSettings", out var storageObj) && storageObj is Dictionary<string, object> storageDict)
            {
                if (storageDict.TryGetValue("SupportedAppsVersionsFile", out var versionsFile))
                    settings.Add(new() { Name = "StorageSettings__SupportedAppsVersionsFile", Value = versionsFile.ToString()! });
            }

            // Common Settings
            if (apiSettings.TryGetValue("Common", out var commonObj) && commonObj is Dictionary<string, object> commonDict)
            {
                foreach (var kvp in commonDict)
                {
                    settings.Add(new() { Name = $"Common__{kvp.Key}", Value = kvp.Value.ToString()! });
                }
            }

            // Service Icons Options
            if (apiSettings.TryGetValue("ServiceIconsOptions", out var serviceIconsObj) && serviceIconsObj is Dictionary<string, object> serviceIconsDict)
            {
                foreach (var kvp in serviceIconsDict)
                {
                    settings.Add(new() { Name = $"ServiceIconsOptions__{kvp.Key}", Value = kvp.Value.ToString()! });
                }
            }

            // Secure values using secrets
            settings.Add(new() { Name = "ApiKey", Value = secrets.ApiKey });
            settings.Add(new() { Name = "PrimaryBlobAccountKey", Value = secrets.PrimaryBlobAccountKey });
            settings.Add(new() { Name = "SecondaryBlobAccountKey", Value = secrets.SecondaryBlobAccountKey });
            settings.Add(new() { Name = "App1BlobAccountKey", Value = secrets.App1BlobAccountKey });
            settings.Add(new() { Name = "App1EventsTopicKey", Value = secrets.App1EventsTopicKey });
        }

        private static ConnStringInfoArgs[] GetApp1ApiConnStrings(App1DeploymentConfigs deploymentConfigs)
        {
            var secrets = deploymentConfigs.Secrets;
            var database = deploymentConfigs.Database;

            return new[]
            {
                new ConnStringInfoArgs
                {
                    Name = "PrimaryDataConnection",
                    Type = ConnectionStringType.SQLAzure,
                    ConnectionString = secrets.PrimaryDbConnection
                },
                new ConnStringInfoArgs
                {
                    Name = "SecondaryDataConnection",
                    Type = ConnectionStringType.SQLAzure,
                    ConnectionString = secrets.SecondaryDbConnection
                },
                new ConnStringInfoArgs
                {
                    Name = "PrimaryBlobStorage",
                    Type = ConnectionStringType.Custom,
                    ConnectionString = secrets.BuildBlobConnectionString(database["PrimaryBlobAccount"], secrets.PrimaryBlobAccountKey)
                },
                new ConnStringInfoArgs
                {
                    Name = "SecondaryBlobStorage",
                    Type = ConnectionStringType.Custom,
                    ConnectionString = secrets.BuildBlobConnectionString(database["SecondaryBlobAccount"], secrets.SecondaryBlobAccountKey)
                },
                new ConnStringInfoArgs
                {
                    Name = "App1BlobStorage",
                    Type = ConnectionStringType.Custom,
                    ConnectionString = secrets.BuildBlobConnectionString(database["App1BlobAccount"], secrets.App1BlobAccountKey)
                }
            };
        }
    }
}

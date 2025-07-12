using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ApiManagement;
using Pulumi.AzureNative.ApiManagement.Inputs;
using Pulumi.AzureNative.Insights;

namespace SharedResources
{
    public partial class Stack
    {
        #region API Management
        private static (ApiManagementService apim, Component insights) CreateApiManagement(DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            var apimConfig = deploymentConfigs.ApiManagement;

            // Application Insights for API Management
            var appInsights = new Component(deploymentConfigs.ResourcesNames["ApiManagementInsightsName"], new ComponentArgs
            {
                ResourceName = $"{deploymentConfigs.Client}-{deploymentConfigs.Prefix}-{deploymentConfigs.ResourcesNames["ApiManagementInsightsName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                ApplicationType = ApplicationType.Web,
                Kind = "web"
            });

            var apiManagement = new ApiManagementService(deploymentConfigs.ResourcesNames["ApiManagementName"], new ApiManagementServiceArgs
            {
                ServiceName = $"{deploymentConfigs.Client}-{deploymentConfigs.Prefix}-{deploymentConfigs.ResourcesNames["ApiManagementName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags,
                Sku = new ApiManagementServiceSkuPropertiesArgs
                {
                    Name = ParseApiManagementSkuName(apimConfig["SkuName"].ToString()!),
                    Capacity = int.Parse(apimConfig["SkuCapacity"].ToString()!)
                },
                PublisherName = apimConfig["PublisherName"].ToString()!,
                PublisherEmail = apimConfig["PublisherEmail"].ToString()!,
                Identity = new ApiManagementServiceIdentityArgs
                {
                    Type = ApimIdentityType.SystemAssigned
                },
                EnableClientCertificate = true  
            });

            // Configure Application Insights Logger for API Management
            var apimLogger = new Logger("apim-insights-logger", new LoggerArgs
            {
                ResourceGroupName = resourceGroup.Name,
                ServiceName = apiManagement.Name,
                LoggerId = "applicationinsights",
                LoggerType = LoggerType.ApplicationInsights,
                Description = "Application Insights logger for API Management",
                Credentials = appInsights.InstrumentationKey.Apply(key => new Dictionary<string, string>
                {
                    ["instrumentationKey"] = key
                }),
                IsBuffered = true
            }, new CustomResourceOptions { DependsOn = { apiManagement, appInsights } });

            return (apiManagement, appInsights);
        }

        private static SkuType ParseApiManagementSkuName(string skuName)
        {
            return skuName.ToLower() switch
            {
                "developer" => SkuType.Developer,
                "basic" => SkuType.Basic,
                "standard" => SkuType.Standard,
                "premium" => SkuType.Premium,
                "consumption" => SkuType.Consumption,
                _ => SkuType.Developer
            };
        }
        #endregion
    }
}
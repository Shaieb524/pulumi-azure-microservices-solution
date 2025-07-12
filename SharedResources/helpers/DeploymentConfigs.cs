using System.Collections.Generic;
using Pulumi;
using SharedResources.helpers;

namespace SharedResources
{
    public record DeploymentConfigs
    {
        public string Location { get; init; }
        public string Prefix { get; init; }
        public string Client { get; init; }
        public string Environment { get; init; }

        // Resource names
        public Dictionary<string, string> ResourcesNames { get; init; }

        // Database names
        public List<string> Databases { get; init; }

        // Tags
        public Dictionary<string, string> CommonTags { get; init; }

        // Plan SKU
        public Dictionary<string, object> PlanSku { get; init; }

        // Storage Account Configuration
        public Dictionary<string, object> StorageAccount { get; init; }

        // Container Registry Configuration
        public Dictionary<string, object> ContainerRegistry { get; init; }

        // API Management Configuration
        public Dictionary<string, object> ApiManagement { get; init; }

        // Secure secret access
        public SecretAccess Secrets { get; init; }

        public DeploymentConfigs(Config config)
        {
            Location = config.Require("Location");
            Prefix = config.Require("Prefix");
            Client = config.Require("Client");
            Environment = config.Require("Environment");

            CommonTags = config.RequireObject<Dictionary<string, string>>("Tags");
            PlanSku = config.RequireObject<Dictionary<string, object>>("PlanSku");
            StorageAccount = config.RequireObject<Dictionary<string, object>>("StorageAccount");
            ContainerRegistry = config.RequireObject<Dictionary<string, object>>("ContainerRegistry");
            ApiManagement = config.RequireObject<Dictionary<string, object>>("ApiManagement");
            ResourcesNames = config.RequireObject<Dictionary<string, string>>("ResourcesNames");
            Databases = config.RequireObject<List<string>>("Databases");

            // Secret access
            Secrets = new SecretAccess(config);
        }
    }
}
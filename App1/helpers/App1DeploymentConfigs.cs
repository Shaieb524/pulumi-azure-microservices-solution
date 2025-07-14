using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using SharedResources.helpers;

namespace App1.helpers
{
    public record App1DeploymentConfigs
    {
        public string Location { get; init; }
        public string Prefix { get; init; }
        public string Environment { get; init; }
        public string Client { get; init; }
        public string DeploymentKind { get; init; }
        public string ConfigNameSeparator { get; init; }

        // Docker settings
        public Dictionary<string, string> DockerSettings { get; init; }
        public string DockerRegistryUrl { get; init; }
        public string DockerRegistryUserName { get; init; }
        public string DockerRegistryPassword { get; init; }
        public string App1ApiLinuxFxVersion { get; init; }
        public string App1FnLinuxFxVersion { get; init; }

        // Resource names
        public Dictionary<string, string> ResourcesNames { get; init; }

        // Storage Account settings
        public Dictionary<string, object> StorageAccount { get; init; }

        // Database settings
        public Dictionary<string, string> Database { get; init; }

        // Tags
        public Dictionary<string, string> CommonTags { get; init; }

        // Plan SKU
        public Dictionary<string, object> PlanSku { get; init; }

        // Your app-specific configurations 
        public Dictionary<string, object> App1ApiAppSettings { get; init; }
        public Dictionary<string, object> App1FnAppSettings { get; init; }

        // Secure secret access - fixed with flat structure
        public App1SecretAccess Secrets { get; init; }

        public App1DeploymentConfigs(Config config)
        {
            // Get from shared config
            var sharedConfig = new Config("SharedResources");
            Location = sharedConfig.Require("Location");
            Prefix = sharedConfig.Require("Prefix");
            Environment = sharedConfig.Require("Environment");
            Client = sharedConfig.Require("Client");
            CommonTags = sharedConfig.RequireObject<Dictionary<string, string>>("Tags");
            PlanSku = sharedConfig.RequireObject<Dictionary<string, object>>("PlanSku");

            // Docker settings from shared config
            var sharedDockerSettings = sharedConfig.RequireObject<Dictionary<string, string>>("DockerSettings");
            DockerRegistryUrl = sharedDockerSettings["DockerRegistryUrl"];
            DockerRegistryUserName = sharedDockerSettings["DockerRegistryUserName"];
            var checkPassSec = sharedDockerSettings.ContainsKey("DockerRegistryPassword")
                ? sharedDockerSettings["DockerRegistryPassword"]
                : "test";
            // Fallback for testing purposes
            DockerRegistryPassword = checkPassSec;

            // App1 specific config
            DeploymentKind = config.Get("DeploymentKind") ?? "standard";
            ResourcesNames = config.RequireObject<Dictionary<string, string>>("ResourcesNames");
            DockerSettings = config.RequireObject<Dictionary<string, string>>("DockerSettings");
            StorageAccount = config.RequireObject<Dictionary<string, object>>("StorageAccount");
            Database = config.RequireObject<Dictionary<string, string>>("Database");

            var apiSettingsRaw = config.RequireObject<JsonElement>("App1ApiAppSettings");
            App1ApiAppSettings = ConfigParser.ConvertJsonElementToDictionary(apiSettingsRaw);

            var fnSettingsRaw = config.RequireObject<JsonElement>("App1FnAppSettings");
            App1FnAppSettings = ConfigParser.ConvertJsonElementToDictionary(fnSettingsRaw);

            // Docker FX versions using shared docker settings
            App1ApiLinuxFxVersion = $"DOCKER|{sharedDockerSettings["DockerRegistryUrl"]}/{DockerSettings["DockerApp1ApiImageName"]}:{DockerSettings["DockerApp1ApiImageTag"]}";
            App1FnLinuxFxVersion = $"DOCKER|{sharedDockerSettings["DockerRegistryUrl"]}/{DockerSettings["DockerApp1FnImageName"]}:{DockerSettings["DockerApp1FnImageTag"]}";

            ConfigNameSeparator = DeploymentKind.ToLower() == "linux" ? "__" : ":";

            // Secret access with shared config
            Secrets = new App1SecretAccess(config, sharedConfig);
        }
    }
}

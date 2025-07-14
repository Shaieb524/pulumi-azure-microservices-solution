using Pulumi;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace App2.helpers
{
    // App2 secret access with shared config support
    public class App2SecretAccess
    {
        private readonly Config _config;
        private readonly Config _sharedConfig;
        private readonly Dictionary<string, string> sharedConnectionStrings;

        public App2SecretAccess(Config config, Config? sharedConfig = null)
        {
            // If sharedConfig is provided, use it; otherwise, use the default config
            _config = config;
            _sharedConfig = sharedConfig ?? config;
            sharedConnectionStrings = _sharedConfig.RequireObject<Dictionary<string, string>>("ConnectionStrings");
        }

        // Direct secret access for App2
        public Output<string> ApiKey => _config.RequireSecret("ApiKey");
        public Output<string> PrimaryBlobAccountKey => _config.RequireSecret("PrimaryBlobAccountKey");
        public Output<string> SecondaryBlobAccountKey => _config.RequireSecret("SecondaryBlobAccountKey");
        public Output<string> App2FnStorageKey => _config.RequireSecret("App2FnStorageKey");

        // Storage Account Keys
        public Output<string> PrimaryBlobKey => _config.RequireSecret("PrimaryBlobKey");
        public Output<string> SecondaryBlobKey => _config.RequireSecret("SecondaryBlobKey");
        public Output<string> App2BlobKey => _config.RequireSecret("App2BlobKey");

        // Database connection strings (retrieved from shared ESC if needed)
        public string PrimaryDbConnection => sharedConnectionStrings.ContainsKey("PrimaryDB") ? sharedConnectionStrings["PrimaryDB"] : "test";
        public string SecondaryDbConnection => sharedConnectionStrings.ContainsKey("SecondaryDB") ? sharedConnectionStrings["SecondaryDB"] : "test";

        public Output<string> BuildBlobConnectionString(string accountName, Output<string> accountKey)
        {
            return accountKey.Apply(key =>
                $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={key};BlobEndpoint=https://{accountName}.blob.core.windows.net/;QueueEndpoint=https://{accountName}.queue.core.windows.net/;TableEndpoint=https://{accountName}.table.core.windows.net/;FileEndpoint=https://{accountName}.file.core.windows.net/;");
        }

        // Helper method to build blob connection string using configuration
        public Output<string> BuildPrimaryBlobConnectionString()
        {
            var databaseObj = _config.RequireObject<Dictionary<string, string>>("Database");
            var accountName = databaseObj.ContainsKey("PrimaryBlobAccount") ? databaseObj["PrimaryBlobAccount"] : throw new ArgumentException("Primary Blob Account not configured");
            return BuildBlobConnectionString(accountName, PrimaryBlobKey);
        }

        public Output<string> BuildSecondaryBlobConnectionString()
        {
            var databaseObj = _config.RequireObject<Dictionary<string, string>>("Database");
            var accountName = databaseObj.ContainsKey("SecondaryBlobAccount") ? databaseObj["SecondaryBlobAccount"] : throw new ArgumentException("Secondary Blob Account not configured");
            return BuildBlobConnectionString(accountName, SecondaryBlobKey);
        }

        public Output<string> BuildApp2BlobConnectionString()
        {
            var databaseObj = _config.RequireObject<Dictionary<string, string>>("Database");
            var accountName = databaseObj.ContainsKey("App2BlobAccount") ? databaseObj["App2BlobAccount"] : throw new ArgumentException("App2 Blob Account not configured");
            return BuildBlobConnectionString(accountName, App2BlobKey);
        }
    }
}

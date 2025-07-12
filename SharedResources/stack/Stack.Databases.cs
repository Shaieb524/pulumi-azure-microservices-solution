using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Sql;
using SharedResources.helpers;

namespace SharedResources
{
    public partial class Stack
    {
        #region SQL Databases
        private static List<Database> CreateDatabases(DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup, Server sqlServer)
        {
            var databases = new List<Database>();

            foreach (var databaseName in deploymentConfigs.Databases)
            {
                var database = new Database(databaseName, new DatabaseArgs
                {
                    DatabaseName = databaseName,
                    ResourceGroupName = resourceGroup.Name,
                    ServerName = sqlServer.Name,
                    Location = resourceGroup.Location,
                    Tags = deploymentConfigs.CommonTags,
                    Sku = new Pulumi.AzureNative.Sql.Inputs.SkuArgs
                    {
                        Name = "Basic",
                        Tier = "Basic"
                    },
                    MaxSizeBytes = 2147483648, // 2GB
                    Collation = "SQL_Latin1_General_CP1_CI_AS"
                });

                databases.Add(database);
            }

            return databases;
        }
        #endregion
    }
}
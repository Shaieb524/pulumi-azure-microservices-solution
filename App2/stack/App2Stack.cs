using Pulumi;
using Pulumi.AzureNative.Resources;
using App2.helpers;

namespace App2.stack
{
    public partial class App2Stack : Stack
    {
        public App2Stack()
        {
            var config = new Config("App2Infrastructure");
            var deploymentConfigs = new App2DeploymentConfigs(config);

            var resourceGroup = new ResourceGroup("resourcegroup", new ResourceGroupArgs
            {
                ResourceGroupName = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["ResourceGroupName"]}",
                Location = deploymentConfigs.Location,
                Tags = deploymentConfigs.CommonTags
            });

            var app2ApiAppService = CreateApp2ApiAppService(deploymentConfigs, resourceGroup);
            var app2FnApp = CreateApp2FunctionApp(deploymentConfigs, resourceGroup);

            ResourceGroupName = resourceGroup.Name;
            App2ApiUrl = app2ApiAppService.DefaultHostName.Apply(hostname => $"https://{hostname}");
            App2ApiAppServiceName = app2ApiAppService.Name;
            App2FunctionUrl = app2FnApp.DefaultHostName.Apply(hostname => $"https://{hostname}");
            App2FunctionAppName = app2FnApp.Name;
        }

        [Output] public Output<string> ResourceGroupName { get; set; }
        [Output] public Output<string> App2ApiUrl { get; set; }
        [Output] public Output<string> App2ApiAppServiceName { get; set; }
        [Output] public Output<string> App2FunctionUrl { get; set; }
        [Output] public Output<string> App2FunctionAppName { get; set; }
    }
}

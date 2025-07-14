using Pulumi;
using Pulumi.AzureNative.Resources;
using App1.helpers;

namespace App1.stack
{
    public partial class App1Stack : Stack
    {
        public App1Stack()
        {
            var config = new Config("App1Infrastructure");
            var deploymentConfigs = new App1DeploymentConfigs(config);

            var resourceGroup = new ResourceGroup("resourcegroup", new ResourceGroupArgs
            {
                ResourceGroupName = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["ResourceGroupName"]}",
                Location = deploymentConfigs.Location,
                Tags = deploymentConfigs.CommonTags
            });

            var app1ApiAppService = CreateApp1ApiAppService(deploymentConfigs, resourceGroup);
            var app1StorageAccount = CreateApp1StorageAccount(deploymentConfigs, resourceGroup);
            var app1FnApp = CreateApp1FunctionApp(deploymentConfigs, resourceGroup);
            var (app1EventsEventGridTopic, app1EventsTopicKey) = CreateApp1EventsEventGridTopic(deploymentConfigs, resourceGroup);

            ResourceGroupName = resourceGroup.Name;
            App1ApiUrl = app1ApiAppService.DefaultHostName.Apply(hostname => $"https://{hostname}");
            App1ApiAppServiceName = app1ApiAppService.Name;
            App1StorageAccountName = app1StorageAccount.Name;
            App1StorageAccountPrimaryEndpoint = app1StorageAccount.PrimaryEndpoints.Apply(endpoints => endpoints.Blob);
            App1FunctionUrl = app1FnApp.DefaultHostName.Apply(hostname => $"https://{hostname}");
            App1FunctionAppName = app1FnApp.Name;
            App1EventsTopicEndpoint = app1EventsEventGridTopic.Endpoint;
            App1EventsTopicKey = app1EventsTopicKey;
        }

        [Output] public Output<string> ResourceGroupName { get; set; }
        [Output] public Output<string> App1ApiUrl { get; set; }
        [Output] public Output<string> App1ApiAppServiceName { get; set; }
        [Output] public Output<string> App1StorageAccountName { get; set; }
        [Output] public Output<string> App1StorageAccountPrimaryEndpoint { get; set; }
        [Output] public Output<string> App1FunctionUrl { get; set; }
        [Output] public Output<string> App1FunctionAppName { get; set; }
        [Output] public Output<string> App1EventsTopicEndpoint { get; set; }
        [Output] public Output<string> App1EventsTopicKey { get; set; }
    }
}

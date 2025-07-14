using Pulumi;
using Pulumi.AzureNative.EventGrid;
using Pulumi.AzureNative.Resources;
using App1.helpers;

namespace App1.stack
{
    public partial class App1Stack
    {
        private static (Topic topic, Output<string> accessKey) CreateApp1EventsEventGridTopic(App1DeploymentConfigs deploymentConfigs, ResourceGroup resourceGroup)
        {
            var eventGridTopic = new Topic($"{deploymentConfigs.ResourcesNames["App1EventsBookingTopicName"]}", new TopicArgs
            {
                TopicName = $"{deploymentConfigs.Client.ToLower()}-{deploymentConfigs.Prefix.ToLower()}-{deploymentConfigs.ResourcesNames["App1EventsBookingTopicName"]}",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Tags = deploymentConfigs.CommonTags
            });

            // Get the access keys for the Event Grid Topic
            var topicKeys = Pulumi.AzureNative.EventGrid.ListTopicSharedAccessKeys.Invoke(new ListTopicSharedAccessKeysInvokeArgs
            {
                ResourceGroupName = resourceGroup.Name,
                TopicName = eventGridTopic.Name
            });

            var accessKey = topicKeys.Apply(keys => keys.Key1 ?? "");

            return (eventGridTopic, accessKey);
        }
    }
}

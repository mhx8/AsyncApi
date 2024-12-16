using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Saunter.Attributes;

namespace AsyncApi.Publisher;

[AsyncApi]
[Channel(
    "cloudeventsyotube/cloudevents",
    BindingsRef = "channel",
    Servers = ["dev"])]
[PublishOperation(
    typeof(CloudEvent),
    ["AsyncApi", "CloudEvent"],
    OperationId = "SendCloudEvent",
    Description = "Publish a event to the ServiceBus.")]
public class EventPublisher(
    IAzureClientFactory<ServiceBusClient> clientFactory,
    IConfiguration configuration)
{
    [Message(typeof(CloudEvent), BindingsRef = "cloudevent")]
    public async Task SendCloudEventAsync(
        CloudEvent cloudEvent)
    {
        ServiceBusClient? client = clientFactory.CreateClient("Default");
        ServiceBusSender? sender = client.CreateSender(configuration["AzureServiceBus:TopicName"]);

        ServiceBusMessage message = new(new BinaryData(cloudEvent))
        {
            ContentType = "application/cloudevents+json"
        };

        await sender.SendMessageAsync(message);
    }
}
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Saunter.Attributes;

namespace AsyncApi.Consumer;

[AsyncApi]
[Channel(
    "cloudeventsyotube/cloudevents",
    BindingsRef = "channel",
    Servers = ["dev"])]
[SubscribeOperation(
    typeof(CloudEvent),
    ["AsyncApi", "CloudEvent"],
    OperationId = "ReceiveCloudEvent",
    Description = "Listen for events on the ServiceBus.")]
public class EventConsumer : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ServiceBusProcessor _processor;

    public EventConsumer(
        IAzureClientFactory<ServiceBusClient> clientFactory,
        ILogger<EventConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        ServiceBusClient? client = clientFactory.CreateClient("Default");
        _processor = client.CreateProcessor(
            configuration["AzureServiceBus:TopicName"],
            "ConsumerSubscription",
            new ServiceBusProcessorOptions());
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    [Message(typeof(CloudEvent), BindingsRef = "cloudevent")]
    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            CloudEvent cloudEvent = CloudEvent.Parse(args.Message.Body)!;
            EventData deserializedData = cloudEvent.Data!.ToObjectFromJson<EventData>()!;

            _logger.LogInformation(
                $"Received CloudEvent: Id={cloudEvent.Id}, Type={cloudEvent.Type}, Message={deserializedData.Message}");

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing message: {ex.Message}");
        }
    }

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError($"Error: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await _processor.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
using AsyncApi.Publisher;
using Azure.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace AsyncApi;

[ApiController]
[Route("api/events")]
public class EventsController(EventPublisher senderService) : ControllerBase
{
    [HttpGet("{message}")]
    public async Task<IActionResult> SendEvent(string message)
    {
        CloudEvent cloudEvent = new(
            source: "AsyncApi",
            type: "AsyncApi.My.CloudEvent",
            jsonSerializableData: new EventData(message)
        );

        await senderService.SendCloudEventAsync(cloudEvent);
        return Ok("Event sent!");
    }
}
using AsyncApi;
using AsyncApi.Consumer;
using AsyncApi.Publisher;
using Microsoft.Extensions.Logging.Console;
using Saunter;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(options =>
{
    options.ColorBehavior = LoggerColorBehavior.Enabled;
    options.SingleLine = true;
})
.AddFilter(
    "Azure",
    LogLevel.None)
.AddFilter(
    "Microsoft",
    LogLevel.None);
builder.Services.AddControllers();
builder.Services.AddAzureServiceBusClients(builder.Configuration);
builder.Services.AddHostedService<EventConsumer>();
builder.Services.AddScoped<EventPublisher>();
builder.Services.AddAsyncApi(builder.Configuration);

WebApplication app = builder.Build();
app.MapControllers();
app.MapAsyncApiDocuments();
app.MapAsyncApiUi();
app.Run();
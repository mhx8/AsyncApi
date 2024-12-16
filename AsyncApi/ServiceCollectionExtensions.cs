using Azure.Identity;
using Microsoft.Extensions.Azure;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.AsyncApiSchema.v2.Bindings;
using Saunter.AsyncApiSchema.v2.Bindings.Amqp;

namespace AsyncApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureServiceBusClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAzureClients(
            clientBuilder =>
            {
                clientBuilder.AddServiceBusClientWithNamespace(configuration["AzureServiceBus:Namespace"])
                    .WithCredential(
                        new ClientSecretCredential(
                            configuration["AzureAD:TenantId"],
                            configuration["AzureAD:ClientId"],
                            configuration["AzureAD:ClientSecret"]));
            });

        return services;
    }

    public static IServiceCollection AddAsyncApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAsyncApiSchemaGeneration(
            options =>
            {
                options.AssemblyMarkerTypes = new[]
                {
                    typeof(ServiceCollectionExtensions),
                };

                options.Middleware.UiTitle = "AsyncApi";
                options.AsyncApi = new AsyncApiDocument
                {
                    Info = new Info(
                        "AsyncApi",
                        "1.0.0")
                    {
                        Description = "Produces and Consumes Messages",
                    },
                    Servers =
                    {
                        ["dev"] = new Server(
                            configuration["AzureServiceBus:Namespace"],
                            "amqp")
                    },
                    Components = new Components
                    {
                        ChannelBindings =
                      {
                          ["channel"] = new ChannelBindings
                          {
                              Amqp = new AmqpChannelBinding
                              {
                                  Exchange = new AmqpChannelBindingExchange
                                  {
                                      Type = AmqpChannelBindingExchangeType.Topic
                                  },
                                  Is = AmqpChannelBindingIs.RoutingKey
                              }
                          }
                      },
                        MessageBindings =
                      {
                            ["cloudevent"] = new MessageBindings
                            {
                                Amqp = new AmqpMessageBinding
                                {
                                    ContentEncoding = "application/cloudevents+json",
                                    MessageType = "application/cloudevents+json"
                                }
                            }
                      }
                    },
                };
            });

        return services;
    }
}
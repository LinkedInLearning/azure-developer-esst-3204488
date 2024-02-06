using System.Net;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace LiL.Azure4Devs.Functions;

[ApiController]
[Route("/api/Product")]
public class ProductRecommendationController : ControllerBase
{
    private readonly ILogger<ProductRecommendationController> _logger;

    public ProductRecommendationController(ILogger<ProductRecommendationController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetProductRecommendation")]
    public async Task<IActionResult> GetProductRecommendation(string productId, string apiVersion="2024-01-01")
    {
        //TODO: ADD your SB Connection string and queue name
        string connectionString = "Endpoint=sb://lil-azure4devs.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=oK8MARl5hJtblDnruqpwx5OylNUzyVGXM+ASbIqhtRI=";
        string queueName = "lil-azure4devs-queue";

        await using var serviceBusClient = new ServiceBusClient(connectionString);
        ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);

        //Create Service Bus Message
        string serviceBusMessage = JsonSerializer.Serialize(
            new { 
                messageId = Guid.NewGuid(), 
                messageType = "ProductRecommendation",
                productId = productId,
                messageDate = DateTime.UtcNow
            }
        );
        ServiceBusMessage message = new ServiceBusMessage(serviceBusMessage);

        await serviceBusSender.SendMessageAsync(message);

        //provide arbitrary response
        var returnValue = new {
            apiVersion = apiVersion,
            productId = productId,
            recommendation = new List<object>{
                new {
                    productId = "123",
                    productDescription = "Product A"
                }, 
                new {
                    productId = "456",
                    productDescription = "Product B"
                }
            }
        };

        return Ok(returnValue);
    }
}

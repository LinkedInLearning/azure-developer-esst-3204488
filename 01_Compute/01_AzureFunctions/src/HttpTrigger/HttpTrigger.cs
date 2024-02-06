using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;


namespace LiL.Azure4Devs.Functions
{
    public static class MultiOutput
    {
        [Function("MultiOutputBinding")]
        public static MultiOutputType Run (
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData httpRequestData,
            FunctionContext context
        )
        {

            string productId = httpRequestData.Query["productId"] ?? "";

            //Create Service Bus Message
            string serviceBusMessage = JsonSerializer.Serialize(
                new { 
                    messageId = Guid.NewGuid(), 
                    messageType = "ProductRecommendation",
                    productId = productId,
                    messageDate = DateTime.UtcNow
                }
            );

            //Create HTTP response
            HttpResponseData httpResponseData = httpRequestData.CreateResponse(HttpStatusCode.OK);
            httpResponseData.WriteString($"Message: {serviceBusMessage} successfully sent to Service Bus");
            
            return new MultiOutputType()
            {
                ServiceBusOutput = serviceBusMessage,
                HttpOutput = httpResponseData
            };
        }
    }

    public class MultiOutputType
    {
        [ServiceBusOutput("lil-azure4devs-queue")]
        public string ServiceBusOutput { get; set; } = "";

        public HttpResponseData? HttpOutput { get; set; } = default(HttpResponseData);  
        
    }

}

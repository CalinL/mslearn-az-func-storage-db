using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace My.Functions
{
    public class HttpExample
    {
        private readonly ILogger _logger;

        public HttpExample(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpExample>();
        }

        [Function("HttpExample")]
        public static MultiResponse Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpExample");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var message = "Welcome to Azure Functions!";

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(message);
             
            // Return a response to HTTP trigger, Azure Cosmos DB output binding, and storage output binding.    
            return new MultiResponse()
            {
                Document = new MyDocument
                {
                    id = System.Guid.NewGuid().ToString(),
                    message = message
                },
                // Write a single message.
                Messages = new string[] { message },
                HttpResponse = response
            };
        }
    }

    public class MultiResponse
    {
        [QueueOutput("outqueue",Connection = "AzureWebJobsStorage")]
        public string[] Messages { get; set; }

        [CosmosDBOutput("my-database", "my-container",
                        ConnectionStringSetting = "CosmosDbConnectionString", CreateIfNotExists = true)]
        public MyDocument Document { get; set; }

        public HttpResponseData HttpResponse { get; set; }
    }
    
    public class MyDocument 
    {
        public string id { get; set; }
        public string message { get; set; }
    }
}

using System.ComponentModel;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderProcessor
{
    public class OrderProcessorFunction
    {
        private readonly ILogger _logger;

        public OrderProcessorFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OrderProcessorFunction>();
        }

        [Function("OrderProcessorFunction")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processing a request.");

            var cosmosClient = new CosmosClient("https://eshoponwebaccount.documents.azure.com:443/", "RGl58Fy2niUw31RAb0zTrJ7Ibkx81qhVYyXpSz1qcbwA2gREf4yBmPKfK6DoTav5E6WES6yDsBQPACDbF8FcVQ==");
            var database = cosmosClient.GetDatabase("OrderDB");
            var container = database.GetContainer("OrderContainer");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<OrderViewModel>(requestBody);

            ItemResponse<OrderViewModel> orderResponse = container.CreateItemAsync<OrderViewModel>(data, new PartitionKey(data.Id)).Result;
            var savedOrder = orderResponse.Resource;


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Oder saved to CosmosDB.");

            return response;
        }
    }
}

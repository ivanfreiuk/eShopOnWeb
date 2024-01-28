using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public class OrderItemsReserverFunction
    {
        private readonly ILogger<OrderItemsReserverFunction> _logger;

        public OrderItemsReserverFunction(ILogger<OrderItemsReserverFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderItemsReserverFunction))]
        public async Task Run([ServiceBusTrigger("orderitemqueue", Connection = "OrderItemQueueConnection")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            try
            {
                UploadOrder(message.Body);
            }
            catch (Exception ex)
            {
                await SendNotificationAsync(ex.Message, message.Body);
            }          
        }

        private void UploadOrder(BinaryData body)
        {
            var blobServiceClient = new BlobServiceClient("BlobEndpoint=https://eshoponwebstorage2024.blob.core.windows.net/eshoponweb?sp=racwdli&st=2024-01-27T21:03:59Z&se=2024-01-28T05:03:59Z&sv=2022-11-02&sr=c&sig=1D6%2Bk%2Bo1MHjQMJDKI50WaYGcCsYtfuQ4IW6W77IQ8CY%3D");
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("eshoponweb");
            var blobClient = blobContainerClient.GetBlobClient($"{Guid.NewGuid()}.json");

            blobClient.Upload(body, true);
        }

        private async Task SendNotificationAsync(string errorMessage, BinaryData body)
        {
            var url = "https://eshoponwebnotificationapp.azurewebsites.net:443/api/SendNotification/triggers/When_a_HTTP_request_is_received/invoke?api-version=2022-05-01&sp=%2Ftriggers%2FWhen_a_HTTP_request_is_received%2Frun&sv=1.0&sig=ktHx4w93jJourlkE_45rhuggBmRGsUAs5-xqzWXlHik";

            using (var client = new HttpClient())
            {
                var requestBody = new
                {
                    errorMessage = errorMessage.ToString(),
                    requestJson = Encoding.UTF8.GetString(body)
                };
                await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
            }
        }
    }
}

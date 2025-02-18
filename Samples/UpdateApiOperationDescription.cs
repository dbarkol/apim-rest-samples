using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using Azure.Identity;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Samples.Models;

namespace Samples
{
    public class UpdateApiOperationDescription
    {
        private readonly ILogger<UpdateApiOperationDescription> _logger;
        private readonly HttpClient _httpClient;

        public UpdateApiOperationDescription(ILogger<UpdateApiOperationDescription> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function("UpdateApiOperationDescription")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("UpdateApiOperationDescription function invoked");

            // Parse the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateRequest = JsonSerializer.Deserialize<UpdateOperationDescriptionRequest>(requestBody);
            if (updateRequest == null || updateRequest.AppId == null || updateRequest.OperationId == null || updateRequest.Description == null)
            {
                return new BadRequestObjectResult("Invalid request payload");
            }

            // Get the access token using Managed Identity
            var token = await Utils.GetAccessTokenWithManagedIdentity();

            // Update the operation description
            var result = await UpdateOperationDescription(token, updateRequest.AppId, updateRequest.OperationId, updateRequest.Description);

            return new OkObjectResult(result);
        }

        private async Task<string> UpdateOperationDescription(string token, string apiId, string operationId, string newDescription)
        {
            // Reference: https://learn.microsoft.com/en-us/rest/api/apimanagement/api-operation/update?view=rest-apimanagement-2024-05-01&tabs=HTTP

            // Get the subscription ID, resource group, and service name from environment variables
            var subscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");
            var resourceGroup = Environment.GetEnvironmentVariable("ResourceGroup");
            var serviceName = Environment.GetEnvironmentVariable("ServiceName");

            // Construct the request URL
            var requestUrl = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ApiManagement/service/{serviceName}/apis/{apiId}/operations/{operationId}?api-version=2024-05-01";

            // Construct the request body
            var requestBody = new
            {
                properties = new
                {
                    description = newDescription
                }
            };

            // Send the PATCH request
            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PatchAsync(requestUrl, requestContent);

            // Check the response status code
            response.EnsureSuccessStatusCode();

            // Return the response content
            return await response.Content.ReadAsStringAsync();
        }

    }
}

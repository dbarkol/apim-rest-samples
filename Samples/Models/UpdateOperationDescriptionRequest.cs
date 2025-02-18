using System.Text.Json.Serialization;

namespace Samples.Models
{
    internal class UpdateOperationDescriptionRequest
    {
        [JsonPropertyName("appId")]
        public required string AppId { get; set; }
        
        [JsonPropertyName("operationId")]
        public required string OperationId { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }
    }       
}

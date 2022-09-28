//using Newtonsoft.Json;
using FileHelpers;
using System.Text.Json.Serialization;

namespace _511Events.Models
{
    [DelimitedRecord(",")]
    public class TrafficEventModel
    {
        [JsonPropertyName(":id")]
        [FieldQuoted]
        public string? Id { get; set; }
        [JsonPropertyName("event_type")]
        [FieldQuoted]
        public string? EventType { get; set; }
        [JsonPropertyName("organization_name")]
        [FieldQuoted]
        public string? OrganizationName { get; set; }
        [JsonPropertyName("facility_name")]
        [FieldQuoted]
        public string? FacilityName { get; set; }
        [JsonPropertyName("direction")]
        [FieldQuoted]
        public string? Direction { get; set; }
        [JsonPropertyName("city")]
        [FieldQuoted]
        public string? City { get; set; }
        [JsonPropertyName("county")]
        [FieldQuoted]
        public string? County { get; set; }
        [JsonPropertyName("state")]
        [FieldQuoted]
        public string? State { get; set; }
        [JsonPropertyName("create_time")]        
        [FieldQuoted]
        [FieldConverter(ConverterKind.Date, "M/d/yyyy HH:mm:ss")]
        public DateTime CreateTime { get; set; }
        [JsonPropertyName("event_description")]
        [FieldQuoted]
        public string? EventDescription { get; set; }
        [JsonPropertyName("responding_organization_id")]
        [FieldQuoted]
        public string? RespondingOrganizationId { get; set; }
        [JsonPropertyName("latitude")]
        [FieldQuoted]
        public decimal Latitude { get; set; }
        [JsonPropertyName("longitude")]
        [FieldQuoted]
        public decimal Longitude { get; set; }
    }
}

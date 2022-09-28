
using System.Text.Json.Serialization;

namespace _511Events.Models
{
    public class CountResult
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}

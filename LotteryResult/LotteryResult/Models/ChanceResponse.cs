using System.Text.Json.Serialization;

namespace LotteryResult.Models
{
    public class ChanceResponse
    {
        [JsonPropertyName("codigo")]
        public string Code { get; set; }
        [JsonPropertyName("numero")]
        public string Number { get; set; }
    }
}

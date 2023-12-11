using Newtonsoft.Json;

namespace LotteryResult.Dtos
{
    public class LotteryDetail
    {
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("result")]
        public string Result { get; set; }
    }
    public class GetResultResponseDto
    {
        [JsonProperty("lottery")]
        public string LotteryName { get; set; }
        [JsonProperty("results")]
        public List<LotteryDetail> Results { get; set; }
    }
}

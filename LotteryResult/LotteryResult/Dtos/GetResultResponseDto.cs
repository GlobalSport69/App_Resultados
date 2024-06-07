﻿using Newtonsoft.Json;

namespace LotteryResult.Dtos
{
    public class LotteryDetail
    {
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("result")]
        public string Result { get; set; }
        [JsonProperty("sorteo")]
        public string Sorteo { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("animal")]
        public string Animal { get; set; }
    }
    public class GetResultResponseDto
    {
        [JsonProperty("lottery")]
        public string LotteryName { get; set; }
        [JsonProperty("results")]
        public List<LotteryDetail> Results { get; set; }
    }
}

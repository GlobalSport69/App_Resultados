using Newtonsoft.Json;

namespace LotteryResult.Models.Gatazo
{
    public partial class Betm3Response
    {
        [JsonProperty("s")]
        public bool Status { get; set; }

        [JsonProperty("d")]
        public Betm3Data[] Data { get; set; }
    }

    public partial class Betm3Data
    {
        //[JsonProperty("c")]
        //public long C { get; set; }

        [JsonProperty("d")]
        public string LotteryName { get; set; }

        [JsonProperty("s")]
        public Betm3Result[] Resultados { get; set; }
    }

    public partial class Betm3Result
    {
        [JsonProperty("c")]
        public string ID { get; set; }

        //[JsonProperty("d")]
        //public string D { get; set; }

        [JsonProperty("n")]
        public string Number { get; set; }

        [JsonProperty("f")]
        public string Animal { get; set; }
    }
}

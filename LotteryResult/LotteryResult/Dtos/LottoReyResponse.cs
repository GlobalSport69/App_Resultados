namespace LotteryResult.Dtos
{
    public class LottoReyData
    {
        public List<ResultadoList> resultado_list { get; set; }
    }

    public class ResultadoList
    {
        public string fecha { get; set; }
        public string sorteo { get; set; }
        public string resultado { get; set; }
    }

    public class LottoReyResponse
    {
        public bool success { get; set; }
        public LottoReyData data { get; set; }
    }
}

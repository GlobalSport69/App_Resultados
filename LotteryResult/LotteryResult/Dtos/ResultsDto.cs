namespace LotteryResult.Dtos
{
    public class ResultsDto
    {
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public IEnumerable<ResultDetailDto> Results { get; set; }
    }
    public class ResultDetailDto
    {
        public string Date { get; set; }
        public string Result { get; set; }
        public string Time { get; set; }
        public string? Sorteo { get; set; }
        public long? PremierID { get; set; }
    }
}

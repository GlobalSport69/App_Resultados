namespace LotteryResult.Dtos
{
    public class ResultsDto
    {
        public string ProductName { get; set; }
        public IEnumerable<ResultDetailDto> Results { get; set; }
    }
    public class ResultDetailDto
    {
        public string Date { get; set; }
        public string Result { get; set; }
        public string Time { get; set; }
        public string? Sorteo { get; set; }
        public LotteryDto Lottery { get; set; }
        public long? PremierID { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
    }

    public class LotteryDto
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public TimeOnly Time { get; set; }
    }
}

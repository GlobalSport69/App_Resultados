namespace LotteryResult.Models.CarruselMillonario
{
    public class CarruselMResultItem
    {
        public string sorteo { get; set; }
        public string resultado { get; set; }
        public string numero { get; set; }
    }

    public class CarruselMResultsResponseDto
    {
        public int status { get; set; }
        public List<CarruselMResultItem> result { get; set; }
    }
}

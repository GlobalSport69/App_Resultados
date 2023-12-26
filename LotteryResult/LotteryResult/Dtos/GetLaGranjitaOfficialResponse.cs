namespace LotteryResult.Dtos
{
    //public class Brand
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Product
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    public class Lottery
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime hour { get; set; }
        public string type { get; set; }
    }

    public class GetLaGranjitaOfficialResponse
    {
        public string result { get; set; }
        public Lottery lottery { get; set; }
        //public Product product { get; set; }
        //public Brand brand { get; set; }
    }
}

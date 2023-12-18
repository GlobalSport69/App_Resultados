namespace LotteryResult.Dtos
{
    public class ProductoJuego
    {
        public string nombre { get; set; }
        public int order { get; set; }
    }

    public class Resultado
    {
        public string fecha { get; set; }
        public Sorteo sorteo { get; set; }
        public string sorteo_numero { get; set; }
        public ProductoJuego producto_juego { get; set; }
        public string resultado { get; set; }
        public string resultado_elemento { get; set; }
    }

    public class TripleCaracasOfficialResponse
    {
        public bool ok { get; set; }
        public List<Resultado> resultados { get; set; }
    }

    public class Sorteo
    {
        public int id { get; set; }
        public string producto { get; set; }
        public string hora { get; set; }
    }
}

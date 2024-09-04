namespace LotteryResult.Models
{
    public class RuletaActivaPublicacionesDetalle
    {
        public string nro { get; set; }
        public string nombre { get; set; }
    }

    public class Loteria
    {
        public List<Publicaciones> publicaciones { get; set; }
    }

    public class Publicaciones
    {
        public string hora { get; set; }
        public RuletaActivaPublicacionesDetalle a { get; set; }
        public RuletaActivaPublicacionesDetalle b { get; set; }
        public RuletaActivaPublicacionesDetalle c { get; set; }
        public RuletaActivaPublicacionesDetalle d { get; set; }
    }

    public class RuletaActivaResponse
    {
        public Loteria loteria { get; set; }
    }
}

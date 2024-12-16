using Hangfire;

namespace LotteryResult.Services
{
    public class ProviderProductMapper
    {
        private TimeZoneInfo _timeZone;
        private TripleZamoranoOfficial tripleZamoranoOfficial;
        private LottoReyOfficial lottoReyOfficial;
        private TripleZuliaOfficial tripleZuliaOfficial;
        private TripleCalienteOfficial tripleCalienteOfficial;
        private ElRucoTriplesBet elRucoTriplesBet;
        private LaRucaOfficial laRucaOfficial;
        private TripleCaracasOfficial tripleCaracasOfficial;
        private SelvaPlusOfficial selvaPlusOfficial;
        private GuacharoActivoOfficial guacharoActivoOfficial;
        private LaGranjitaOfficial laGranjitaOfficial;
        private LaRicachonaOfficial laRicachonaOfficial;
        private LaGranjitaTerminalOfficial laGranjitaTerminalOfficial;
        private LaRicachonaAnimalitosOfficial laRicachonaAnimalitosOfficial;
        private TripleBombaOfficial tripleBombaOfficial;
        private ChanceAnimalitosOfficial chanceAnimalitosOfficial;
        private TripleChanceOfficial tripleChanceOfficial;
        private TripleTachiraOfficial tripleTachiraOfficial;
        private LottoActivoOfficial lottoActivoOfficial;
        private RuletaActivaOfficial ruletaActivaOfficial;
        private GranjaPlusOfficial granjaPlusOfficial;
        private LottoActivoRDInternacionalOfficial lottoActivoRDInternacionalOfficial;
        private TrioActivoOfficial trioActivoOfficial;
        private CarruselMillonario carruselMillonario;
        private MegaAnimalOfficial megaAnimalOfficial;
        private CazalotonOfficial cazalotonOfficial;
        private GranjaMillonariaOfficial granjaMillonariaOfficial;
        private GranjazoOfficial granjazoOfficial;
        private LottoGatoOfficial lottoGatoOfficial;
        private GatazoOfficial gatazoOfficial;
        private UnelotonOfficial unelotonOfficial;
        public ProviderProductMapper(TripleZamoranoOfficial tripleZamoranoOfficial, LottoReyOfficial lottoReyOfficial, TripleZuliaOfficial tripleZuliaOfficial,
            TripleCalienteOfficial tripleCalienteOfficial, ElRucoTriplesBet elRucoTriplesBet, LaRucaOfficial laRucaOfficial, TripleCaracasOfficial tripleCaracasOfficial,
            SelvaPlusOfficial selvaPlusOfficial, GuacharoActivoOfficial guacharoActivoOfficial, LaGranjitaOfficial laGranjitaOfficial, LaRicachonaOfficial laRicachonaOfficial,
            LaGranjitaTerminalOfficial laGranjitaTerminalOfficial, LaRicachonaAnimalitosOfficial laRicachonaAnimalitosOfficial, TripleBombaOfficial tripleBombaOfficial,
            ChanceAnimalitosOfficial chanceAnimalitosOfficial, TripleChanceOfficial tripleChanceOfficial,
            TripleTachiraOfficial tripleTachiraOfficial, LottoActivoOfficial lottoActivoOfficial, RuletaActivaOfficial ruletaActivaOfficial, GranjaPlusOfficial granjaPlusOfficial,
            LottoActivoRDInternacionalOfficial lottoActivoRDInternacionalOfficial, TrioActivoOfficial trioActivoOfficial, CarruselMillonario carruselMillonario,
            MegaAnimalOfficial megaAnimalOfficial, CazalotonOfficial cazalotonOfficial, GranjaMillonariaOfficial granjaMillonariaOfficial, GranjazoOfficial granjazoOfficial, LottoGatoOfficial lottoGatoOfficial, GatazoOfficial gatazoOfficial, UnelotonOfficial unelotonOfficial)
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");

            this.tripleZamoranoOfficial = tripleZamoranoOfficial;
            this.lottoReyOfficial = lottoReyOfficial;
            this.tripleZuliaOfficial = tripleZuliaOfficial;
            this.tripleCalienteOfficial = tripleCalienteOfficial;
            this.elRucoTriplesBet = elRucoTriplesBet;
            this.laRucaOfficial = laRucaOfficial;
            this.tripleCaracasOfficial = tripleCaracasOfficial;
            this.selvaPlusOfficial = selvaPlusOfficial;
            this.guacharoActivoOfficial = guacharoActivoOfficial;
            this.laGranjitaOfficial = laGranjitaOfficial;
            this.laRicachonaOfficial = laRicachonaOfficial;
            this.laGranjitaTerminalOfficial = laGranjitaTerminalOfficial;
            this.laRicachonaAnimalitosOfficial = laRicachonaAnimalitosOfficial;
            this.tripleBombaOfficial = tripleBombaOfficial;
            this.chanceAnimalitosOfficial = chanceAnimalitosOfficial;
            this.tripleChanceOfficial = tripleChanceOfficial;
            this.tripleTachiraOfficial = tripleTachiraOfficial;
            this.lottoActivoOfficial = lottoActivoOfficial;
            this.ruletaActivaOfficial = ruletaActivaOfficial;
            this.granjaPlusOfficial = granjaPlusOfficial;
            this.lottoActivoRDInternacionalOfficial = lottoActivoRDInternacionalOfficial;
            this.trioActivoOfficial = trioActivoOfficial;
            this.carruselMillonario = carruselMillonario;
            this.megaAnimalOfficial = megaAnimalOfficial;
            this.cazalotonOfficial = cazalotonOfficial;
            this.granjaMillonariaOfficial = granjaMillonariaOfficial;
            this.granjazoOfficial = granjazoOfficial;
            this.lottoGatoOfficial = lottoGatoOfficial;
            this.gatazoOfficial = gatazoOfficial;
            this.unelotonOfficial = unelotonOfficial;
        }

        public void AddJob(int product_id, string job_id, string cron_expression)
        {
            IGetResult job = null;

            if (product_id == TripleZamoranoOfficial.productID)
            {
                job = tripleZamoranoOfficial;
            }

            if (product_id == TripleCaracasOfficial.productID)
            {
                job = tripleCaracasOfficial;
            }

            if (product_id == TripleCalienteOfficial.productID)
            {
                job = tripleCalienteOfficial;
            }

            if (product_id == LottoReyOfficial.productID)
            {
                job = lottoReyOfficial;
            }

            if (product_id == TripleZuliaOfficial.productID)
            {
                job = tripleZuliaOfficial;
            }

            if (product_id == ElRucoTriplesBet.productID)
            {
                job = elRucoTriplesBet;
            }

            if (product_id == LaRucaOfficial.productID)
            {
                job = laRucaOfficial;
            }

            if (product_id == SelvaPlusOfficial.productID)
            {
                job = selvaPlusOfficial;
            }

            if (product_id == GuacharoActivoOfficial.productID)
            {
                job = guacharoActivoOfficial;
            }

            if (product_id == LaGranjitaOfficial.laGranjitaID)
            {
                job = laGranjitaOfficial;
            }

            if (product_id == LaRicachonaOfficial.productID)
            {
                job = laRicachonaOfficial;
            }

            if (product_id == LaGranjitaTerminalOfficial.productID)
            {
                job = laGranjitaTerminalOfficial;
            }

            if (product_id == LaRicachonaAnimalitosOfficial.laRicachonaAnimalitosID)
            {
                job = laRicachonaAnimalitosOfficial;
            }

            if (product_id == TripleBombaOfficial.productID)
            {
                job = tripleBombaOfficial;
            }

            if (product_id == ChanceAnimalitosOfficial.productID)
            {
                job = chanceAnimalitosOfficial;
            }

            if (product_id == TripleChanceOfficial.productID)
            {
                job = tripleChanceOfficial;
            }

            if (product_id == TripleTachiraOfficial.productID)
            {
                job = tripleTachiraOfficial;
            }

            if (product_id == LottoActivoOfficial.productID)
            {
                job = lottoActivoOfficial;
            }

            if (product_id == RuletaActivaOfficial.productID)
            {
                job = ruletaActivaOfficial;
            }

            if (product_id == GranjaPlusOfficial.productID)
            {
                job = granjaPlusOfficial;
            }

            if (product_id == LottoActivoRDInternacionalOfficial.productID)
            {
                job = lottoActivoRDInternacionalOfficial;
            }
            
            if (product_id == TrioActivoOfficial.productID)
            {
                job = trioActivoOfficial;
            }

            if (product_id == CarruselMillonario.productID)
            {
                job = carruselMillonario;
            }
            
            if (product_id == MegaAnimalOfficial.productID)
            {
                job = megaAnimalOfficial;
            }
            
            if (product_id == CazalotonOfficial.productID)
            {
                job = cazalotonOfficial;
            }
            
            if (product_id == GranjaMillonariaOfficial.productID)
            {
                job = granjaMillonariaOfficial;
            }

            if (product_id == GranjazoOfficial.productID)
            {
                job = granjazoOfficial;
            }
            
            if (product_id == LottoGatoOfficial.productID)
            {
                job = lottoGatoOfficial;
            }
            
            if (product_id == GatazoOfficial.productID)
            {
                job = gatazoOfficial;
            }
            
            if (product_id == UnelotonOfficial.productID)
            {
                job = unelotonOfficial;
            }

            if (job != null)
            {
                RecurringJob.AddOrUpdate(job_id,
                    "default",
                    () => job.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone,
                    });
            }
        }

        public void DeleteJob(string job_id)
        {
            RecurringJob.RemoveIfExists(job_id);
        }
    }
}

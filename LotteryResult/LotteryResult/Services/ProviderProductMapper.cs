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
        private ZodiacalCaracasOfficial zodiacalCaracasOfficial;
        private TripleTachiraOfficial tripleTachiraOfficial;
        private TachiraZodiacalOfficial tachiraZodiacalOfficial;
        private ChanceAstralOfficial chanceAstralOfficial;
        private AstroZamoranoOfficial astroZamoranoOfficial;
        private ZodiacoDelZuliaOfficial zodiacoDelZuliaOfficial;
        private SignoCalienteOfficial signoCalienteOfficial;
        private LottoActivoOfficial lottoActivoOfficial;
        private RuletaActivaOfficial ruletaActivaOfficial;
        private GranjaPlusOfficial granjaPlusOfficial;
        private LottoActivoRDInternacionalOfficial lottoActivoRDInternacionalOfficial;
        public ProviderProductMapper(TripleZamoranoOfficial tripleZamoranoOfficial, LottoReyOfficial lottoReyOfficial, TripleZuliaOfficial tripleZuliaOfficial,
            TripleCalienteOfficial tripleCalienteOfficial, ElRucoTriplesBet elRucoTriplesBet, LaRucaOfficial laRucaOfficial, TripleCaracasOfficial tripleCaracasOfficial,
            SelvaPlusOfficial selvaPlusOfficial, GuacharoActivoOfficial guacharoActivoOfficial, LaGranjitaOfficial laGranjitaOfficial, LaRicachonaOfficial laRicachonaOfficial,
            LaGranjitaTerminalOfficial laGranjitaTerminalOfficial, LaRicachonaAnimalitosOfficial laRicachonaAnimalitosOfficial, TripleBombaOfficial tripleBombaOfficial,
            ChanceAnimalitosOfficial chanceAnimalitosOfficial, TripleChanceOfficial tripleChanceOfficial, ZodiacalCaracasOfficial zodiacalCaracasOfficial,
            TripleTachiraOfficial tripleTachiraOfficial, TachiraZodiacalOfficial tachiraZodiacalOfficial, ChanceAstralOfficial chanceAstralOfficial,
            AstroZamoranoOfficial astroZamoranoOfficial, ZodiacoDelZuliaOfficial zodiacoDelZuliaOfficial, SignoCalienteOfficial signoCalienteOfficial,
            LottoActivoOfficial lottoActivoOfficial, RuletaActivaOfficial ruletaActivaOfficial, GranjaPlusOfficial granjaPlusOfficial, LottoActivoRDInternacionalOfficial lottoActivoRDInternacionalOfficial)
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
            this.zodiacalCaracasOfficial = zodiacalCaracasOfficial;
            this.tripleTachiraOfficial = tripleTachiraOfficial;
            this.tachiraZodiacalOfficial = tachiraZodiacalOfficial;
            this.chanceAstralOfficial = chanceAstralOfficial;
            this.astroZamoranoOfficial = astroZamoranoOfficial;
            this.zodiacoDelZuliaOfficial = zodiacoDelZuliaOfficial;
            this.signoCalienteOfficial = signoCalienteOfficial;
            this.lottoActivoOfficial = lottoActivoOfficial;
            this.ruletaActivaOfficial = ruletaActivaOfficial;
            this.granjaPlusOfficial = granjaPlusOfficial;
            this.lottoActivoRDInternacionalOfficial = lottoActivoRDInternacionalOfficial;
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

            if (product_id == ZodiacalCaracasOfficial.productID)
            {
                job = zodiacalCaracasOfficial;
            }

            if (product_id == TripleTachiraOfficial.productID)
            {
                job = tripleTachiraOfficial;
            }

            if (product_id == TachiraZodiacalOfficial.productID)
            {
                job = tachiraZodiacalOfficial;
            }

            if (product_id == ChanceAstralOfficial.productID)
            {
                job = chanceAstralOfficial;
            }

            if (product_id == AstroZamoranoOfficial.productID)
            {
                job = astroZamoranoOfficial;
            }

            if (product_id == ZodiacoDelZuliaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => zodiacoDelZuliaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == SignoCalienteOfficial.productID)
            {
                job = signoCalienteOfficial;
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

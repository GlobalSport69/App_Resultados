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
        public ProviderProductMapper(TripleZamoranoOfficial tripleZamoranoOfficial, LottoReyOfficial lottoReyOfficial, TripleZuliaOfficial tripleZuliaOfficial,
            TripleCalienteOfficial tripleCalienteOfficial, ElRucoTriplesBet elRucoTriplesBet, LaRucaOfficial laRucaOfficial, TripleCaracasOfficial tripleCaracasOfficial,
            SelvaPlusOfficial selvaPlusOfficial, GuacharoActivoOfficial guacharoActivoOfficial, LaGranjitaOfficial laGranjitaOfficial, LaRicachonaOfficial laRicachonaOfficial,
            LaGranjitaTerminalOfficial laGranjitaTerminalOfficial, LaRicachonaAnimalitosOfficial laRicachonaAnimalitosOfficial, TripleBombaOfficial tripleBombaOfficial,
            ChanceAnimalitosOfficial chanceAnimalitosOfficial, TripleChanceOfficial tripleChanceOfficial, ZodiacalCaracasOfficial zodiacalCaracasOfficial,
            TripleTachiraOfficial tripleTachiraOfficial, TachiraZodiacalOfficial tachiraZodiacalOfficial, ChanceAstralOfficial chanceAstralOfficial,
            AstroZamoranoOfficial astroZamoranoOfficial, ZodiacoDelZuliaOfficial zodiacoDelZuliaOfficial, SignoCalienteOfficial signoCalienteOfficial,
            LottoActivoOfficial lottoActivoOfficial, RuletaActivaOfficial ruletaActivaOfficial, GranjaPlusOfficial granjaPlusOfficial)
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
        }

        public void AddJob(int product_id, string job_id, string cron_expression) {

            if (product_id == TripleZamoranoOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id, 
                    () => tripleZamoranoOfficial.Handler(), 
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TripleCaracasOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleCaracasOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TripleCalienteOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleCalienteOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == LottoReyOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => lottoReyOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TripleZuliaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleZuliaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == ElRucoTriplesBet.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => elRucoTriplesBet.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
            }

            if (product_id == LaRucaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => laRucaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == SelvaPlusOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => selvaPlusOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == GuacharoActivoOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => guacharoActivoOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == LaGranjitaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => laGranjitaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == LaRicachonaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => laRicachonaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == LaGranjitaTerminalOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => laGranjitaTerminalOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == LaRicachonaAnimalitosOfficial.laRicachonaAnimalitosID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => laRicachonaAnimalitosOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TripleBombaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleBombaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == ChanceAnimalitosOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => chanceAnimalitosOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TripleChanceOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleChanceOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == ZodiacalCaracasOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => zodiacalCaracasOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TripleTachiraOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleTachiraOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == TachiraZodiacalOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tachiraZodiacalOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == ChanceAstralOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => chanceAstralOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == AstroZamoranoOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => astroZamoranoOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
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
                RecurringJob.AddOrUpdate(job_id,
                    () => signoCalienteOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }
            
            if (product_id == LottoActivoOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => lottoActivoOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            
            if (product_id == RuletaActivaOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => ruletaActivaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }

            if (product_id == GranjaPlusOfficial.productID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => granjaPlusOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
                return;
            }
        }

        public void DeleteJob(string job_id)
        {
            RecurringJob.RemoveIfExists(job_id);
        }
    }
}

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
        public ProviderProductMapper(TripleZamoranoOfficial tripleZamoranoOfficial, LottoReyOfficial lottoReyOfficial, TripleZuliaOfficial tripleZuliaOfficial, TripleCalienteOfficial tripleCalienteOfficial, ElRucoTriplesBet elRucoTriplesBet, LaRucaOfficial laRucaOfficial, TripleCaracasOfficial tripleCaracasOfficial, SelvaPlusOfficial selvaPlusOfficial, GuacharoActivoOfficial guacharoActivoOfficial, LaGranjitaOfficial laGranjitaOfficial, LaRicachonaOfficial laRicachonaOfficial, LaGranjitaTerminalOfficial laGranjitaTerminalOfficial, LaRicachonaAnimalitosOfficial laRicachonaAnimalitosOfficial, TripleBombaOfficial tripleBombaOfficial, ChanceAnimalitosOfficial chanceAnimalitosOfficial, TripleChanceOfficial tripleChanceOfficial, ZodiacalCaracasOfficial zodiacalCaracasOfficial, TripleTachiraOfficial tripleTachiraOfficial = null, TachiraZodiacalOfficial tachiraZodiacalOfficial = null)
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
        }

        public void AddJob(int product_id, string job_id, string cron_expression) {

            if (product_id == TripleZamoranoOfficial.zamoranoID)
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

            if (product_id == TripleCaracasOfficial.tripleCaracasID)
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

            if (product_id == TripleCalienteOfficial.tripleCalienteID)
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

            if (product_id == LottoReyOfficial.lottoReyID)
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

            if (product_id == TripleZuliaOfficial.tripleZuliaID)
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

            if (product_id == ElRucoTriplesBet.elRucoID)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => elRucoTriplesBet.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = _timeZone
                    });
            }

            if (product_id == LaRucaOfficial.laRucaID)
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

            if (product_id == SelvaPlusOfficial.selvaPlusID)
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

            if (product_id == GuacharoActivoOfficial.guacharoID)
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

            if (product_id == LaGranjitaOfficial.laGranjitaID)
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

            if (product_id == LaRicachonaOfficial.laRicachonaID)
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

            if (product_id == LaGranjitaTerminalOfficial.laGranjitaTerminalesID)
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

            if (product_id == TripleBombaOfficial.tripleBombaID)
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

            if (product_id == ChanceAnimalitosOfficial.chanceAnimalitosID)
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
        }

        public void DeleteJob(string job_id)
        {
            RecurringJob.RemoveIfExists(job_id);
        }
    }
}

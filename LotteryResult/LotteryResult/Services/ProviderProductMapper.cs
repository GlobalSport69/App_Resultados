using Hangfire;

namespace LotteryResult.Services
{
    public class ProviderProductMapper
    {
        private TripleZamoranoOfficial tripleZamoranoOfficial;
        private LottoReyOfficial lottoReyOfficial;
        private TripleZuliaOfficial tripleZuliaOfficial;
        private TripleCalienteOfficial tripleCalienteOfficial;
        private ElRucoOfficial elRucoOfficial;
        private LaRucaOfficial laRucaOfficial;
        private TripleCaracasOfficial tripleCaracasOfficial;
        private SelvaPlusOfficial selvaPlusOfficial;
        private GuacharoActivoOfficial guacharoActivoOfficial;
        public ProviderProductMapper(TripleZamoranoOfficial tripleZamoranoOfficial, LottoReyOfficial lottoReyOfficial, TripleZuliaOfficial tripleZuliaOfficial, TripleCalienteOfficial tripleCalienteOfficial, ElRucoOfficial elRucoOfficial, LaRucaOfficial laRucaOfficial, TripleCaracasOfficial tripleCaracasOfficial, SelvaPlusOfficial selvaPlusOfficial, GuacharoActivoOfficial guacharoActivoOfficial)
        {
            this.tripleZamoranoOfficial = tripleZamoranoOfficial;
            this.lottoReyOfficial = lottoReyOfficial;
            this.tripleZuliaOfficial = tripleZuliaOfficial;
            this.tripleCalienteOfficial = tripleCalienteOfficial;
            this.elRucoOfficial = elRucoOfficial;
            this.laRucaOfficial = laRucaOfficial;
            this.tripleCaracasOfficial = tripleCaracasOfficial;
            this.selvaPlusOfficial = selvaPlusOfficial;
            this.guacharoActivoOfficial = guacharoActivoOfficial;
        }

        public void AddJob(int product_id, string job_id, string cron_expression) {

            if (product_id == 2)
            {
                RecurringJob.AddOrUpdate(job_id, 
                    () => tripleZamoranoOfficial.Handler(), 
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 3)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleCaracasOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 4)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleCalienteOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 5)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => lottoReyOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 6)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => tripleZuliaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 8)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => elRucoOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
            }

            if (product_id == 9)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => laRucaOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 10)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => selvaPlusOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
                    });
                return;
            }

            if (product_id == 11)
            {
                RecurringJob.AddOrUpdate(job_id,
                    () => guacharoActivoOfficial.Handler(),
                    cron_expression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time")
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

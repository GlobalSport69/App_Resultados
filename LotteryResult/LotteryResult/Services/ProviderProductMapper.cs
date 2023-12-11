using Hangfire;

namespace LotteryResult.Services
{
    public class ProviderProductMapper
    {
        private TripleZamoranoOfficial tripleZamoranoOfficial;
        private LottoReyOfficial lottoReyOfficial;
        public ProviderProductMapper(TripleZamoranoOfficial tripleZamoranoOfficial, LottoReyOfficial lottoReyOfficial)
        {
            this.tripleZamoranoOfficial = tripleZamoranoOfficial;
            this.lottoReyOfficial = lottoReyOfficial;
        }

        public void AddJob(int product_id, string job_id, string cron_expression) {

            if (product_id == 2)
            {
                RecurringJob.AddOrUpdate(job_id, 
                    () => tripleZamoranoOfficial.Handel(), 
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
                    () => lottoReyOfficial.Handel(),
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

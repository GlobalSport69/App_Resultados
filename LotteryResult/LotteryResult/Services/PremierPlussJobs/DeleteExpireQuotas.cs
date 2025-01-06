using Flurl.Http;

namespace LotteryResult.Services.PremierPlussJobs
{
    public class DeleteExpireQuotas
    {
        private ILogger<DeleteExpireQuotas> _logger;
        private IConfiguration _configuration;

        public const string CronExpression = "56 23 * * *";

        public DeleteExpireQuotas(ILogger<DeleteExpireQuotas> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Handler()
        {
            try
            {
                var venezuelaNow = DateTime.Now;
                var hookUrl = _configuration.GetSection("loteriaHost").Value ?? string.Empty;
                if (string.IsNullOrEmpty(hookUrl))
                    throw new ArgumentException("loteriaHost is empty");

                var response = await $"{hookUrl}/new-quotas/deleteExpiredQuotas"
                .PostJsonAsync(new { })
                .ReceiveString();

                _logger.LogInformation("En {0} se obtuvo: {1}", nameof(DeleteExpireQuotas), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(DeleteExpireQuotas));
                throw;
            }
        }
    }
}

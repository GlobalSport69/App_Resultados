using Flurl.Http;

namespace LotteryResult.Services.PremierPlussJobs
{
    public class SetLimitForIntegrations
    {
        private ILogger<SetLimitForIntegrations> _logger;
        private IConfiguration _configuration;

        public const string CronExpression = "0 0,6,12,16 * * *";

        public SetLimitForIntegrations(ILogger<SetLimitForIntegrations> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Handler()
        {
            try
            {
                var venezuelaNow = DateTime.Now;
                var hookUrl = _configuration.GetSection("PremierApiLimitHookUrl").Value ?? string.Empty;
                if (string.IsNullOrEmpty(hookUrl))
                    throw new ArgumentException("PremierApiLimitHookUrl is empty");

                var response = await $"{hookUrl}"
                .PostJsonAsync(new { })
                .ReceiveString();

                _logger.LogInformation("En {0} se obtuvo: {1}", nameof(SetLimitForIntegrations), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(SetLimitForIntegrations));
                throw;
            }
        }
    }
}

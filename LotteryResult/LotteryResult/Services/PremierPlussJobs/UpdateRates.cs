using Flurl.Http;

namespace LotteryResult.Services.PremierPlussJobs
{
    public class UpdateRates
    {
        private ILogger<UpdateRates> _logger;
        private IConfiguration _configuration;

        public const string CronExpression = "30 9,15 * * *";

        public UpdateRates(ILogger<UpdateRates> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Handler()
        {
            try
            {
                var venezuelaNow = DateTime.Now;
                var hookUrl = _configuration.GetSection("PremierUpdateRateHookUrl").Value ?? string.Empty;
                if (string.IsNullOrEmpty(hookUrl))
                    throw new ArgumentException("PremierUpdateRateHookUrl is empty");

                var response = await $"{hookUrl}"
                .PostJsonAsync(new { })
                .ReceiveString();

                _logger.LogInformation("En {0} se obtuvo: {1}", nameof(UpdateRates), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(UpdateRates));
                throw;
            }
        }
    }
}

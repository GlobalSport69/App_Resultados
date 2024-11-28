using Flurl.Http;

namespace LotteryResult.Services.PremierPlussJobs.CloseNotification
{
    public class SendCloseLotteryNotifyToPremierApi
    {
        private ILogger<SendCloseLotteryNotifyToPremierApi> _logger;
        private IConfiguration _configuration;

        public SendCloseLotteryNotifyToPremierApi(ILogger<SendCloseLotteryNotifyToPremierApi> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Handler(long premierLotteryID)
        {
            try
            {
                var venezuelaNow = DateTime.Now;
                var hookUrl = _configuration.GetSection("PremierApiHookUrl").Value ?? string.Empty;
                if (string.IsNullOrEmpty(hookUrl))
                    throw new ArgumentException("PremierApiHookUrl is empty");

                var response = await $"{hookUrl}{premierLotteryID}"
                .PostJsonAsync(new { })
                .ReceiveString();

                _logger.LogInformation("En {0} se obtuvo: {1}", nameof(SendCloseLotteryNotifyToPremierApi), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(SendCloseLotteryNotifyToPremierApi));
                throw;
            }
        }
    }
}

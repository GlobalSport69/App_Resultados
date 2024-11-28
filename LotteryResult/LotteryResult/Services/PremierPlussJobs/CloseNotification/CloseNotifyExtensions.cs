namespace LotteryResult.Services.PremierPlussJobs.CloseNotification
{
    public static class CloseNotifyExtensions
    {
        public static IServiceCollection AddCloseNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<Guacharo>();
            services.AddScoped<SelvaPlus>();
            services.AddScoped<SendCloseLotteryNotifyToPremierApi>();

            return services;
        }
    }
}

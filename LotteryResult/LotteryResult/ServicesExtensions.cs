using LotteryResult.Data.Abstractions;
using LotteryResult.Services;

namespace LotteryResult
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<IBootstrapJobs, BootstrapJobs>();
            services.AddScoped<LottoReyOfficial>();
            services.AddScoped<TripleZamoranoOfficial>();
            //services.AddScoped<LoteriaDeHoy>();
            services.AddScoped<ProviderProductMapper>();

            return services;
        }
    }
}

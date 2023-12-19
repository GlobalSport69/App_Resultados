using LotteryResult.Data.Abstractions;
using LotteryResult.Services;

namespace LotteryResult
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<LottoReyOfficial>();
            services.AddScoped<TripleZamoranoOfficial>();
            services.AddScoped<TripleZuliaOfficial>();
            services.AddScoped<TripleCalienteOfficial>();
            services.AddScoped<ElRucoOfficial>();
            services.AddScoped<LaRucaOfficial>();
            services.AddScoped<TripleCaracasOfficial>();
            services.AddScoped<SelvaPlusOfficial>();
            services.AddScoped<GuacharoActivoOfficial>();

            services.AddScoped<ProviderProductMapper>();

            return services;
        }
    }
}

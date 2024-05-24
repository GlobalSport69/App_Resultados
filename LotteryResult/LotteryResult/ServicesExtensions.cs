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
            services.AddScoped<ElRucoTriplesBet>();
            services.AddScoped<LaRucaOfficial>();
            services.AddScoped<TripleCaracasOfficial>();
            services.AddScoped<SelvaPlusOfficial>();
            services.AddScoped<GuacharoActivoOfficial>();
            services.AddScoped<LaGranjitaOfficial>();
            services.AddScoped<LaRicachonaOfficial>();
            services.AddScoped<LaGranjitaTerminalOfficial>();
            services.AddScoped<LaRicachonaAnimalitosOfficial>();
            services.AddScoped<TripleBombaOfficial>();
            services.AddScoped<ChanceAnimalitosOfficial>();
            services.AddScoped<TripleChanceOfficial>();

            services.AddScoped<TripleTachiraOfficial>();
            services.AddScoped<TachiraZodiacalOfficial>();
            services.AddScoped<ZodiacalCaracasOfficial>();
            services.AddScoped<ChanceAstralOfficial>();
            services.AddScoped<AstroZamoranoOfficial>();
            services.AddScoped<ZodiacoDelZuliaOfficial>();
            services.AddScoped<SignoCalienteOfficial>();
            services.AddScoped<LottoActivoOfficial>();

            services.AddScoped<ProviderProductMapper>();

            return services;
        }
    }
}

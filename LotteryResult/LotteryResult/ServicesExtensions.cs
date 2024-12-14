using LotteryResult.Data.Abstractions;
using LotteryResult.Services;
using LotteryResult.Services.PremierPlussJobs;
using LotteryResult.Services.PremierPlussJobs.CloseNotification;

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
            services.AddScoped<LottoActivoOfficial>();
            services.AddScoped<RuletaActivaOfficial>();
            services.AddScoped<GranjaPlusOfficial>();
            services.AddScoped<LottoActivoRDInternacionalOfficial>();
            services.AddScoped<TrioActivoOfficial>();
            services.AddScoped<CarruselMillonario>();
            services.AddScoped<MegaAnimalOfficial>();
            services.AddScoped<CazalotonOfficial>();
            services.AddScoped<GranjaMillonariaOfficial>();
            services.AddScoped<GranjazoOfficial>();
            services.AddScoped<LottoGatoOfficial>();

            services.AddScoped<INotifyPremierService, NotifyPremierService>();
            services.AddScoped<ProviderProductMapper>();

            services.AddCloseNotificationServices();
            services.AddScoped<SetLimitForIntegrations>();
            services.AddScoped<UpdateRates>();

            return services;
        }
    }
}

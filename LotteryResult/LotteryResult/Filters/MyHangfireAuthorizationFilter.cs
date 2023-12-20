using Hangfire.Dashboard;

namespace LotteryResult.Filters
{
    public class MyHangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true; // Autoriza todas las solicitudes. No recomendado para producción.
        }
    }
}

using Hangfire.Dashboard;

namespace LotteryResult.Filters
{
    public class MyHangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var _context = context.GetHttpContext();
            if (_context.User == null || !_context.User.Identity.IsAuthenticated)
            {
                //_context.Response.Redirect("/auth/login");
                return false;
            }
            return true;
            //return _context.User != null && _context.User.Identity.IsAuthenticated;

            //return true; // Autoriza todas las solicitudes. No recomendado para producción.
        }
    }
}

using Hangfire;
using LotteryResult.Services.CloseNotification;
using Microsoft.AspNetCore.Mvc;

namespace LotteryResult.Controllers
{
    [Route("Close")]
    public class CloseNotifyController : Controller
    {
        private Guacharo guacharo;
        private SelvaPlus selva;

        public CloseNotifyController(Guacharo guacharo, SelvaPlus selva)
        {
            this.guacharo = guacharo;
            this.selva = selva;
        }


        // GET: Close?product_id=1&status=true
        [HttpGet]
        public ActionResult EnableNotifyClose([FromQuery] int product_id, [FromQuery] bool status = true)
        {
            var _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");
            if (Guacharo.productID == product_id)
            {
                if (status)
                {
                    RecurringJob.AddOrUpdate("Guacharo_notify",
                        "default",
                        () => guacharo.Handler(),
                        Guacharo.CronExpression,
                        new RecurringJobOptions
                        {
                            TimeZone = _timeZone,
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists("Guacharo_notify");
                }
            }

            if (SelvaPlus.productID == product_id)
            {
                if (status)
                {
                    RecurringJob.AddOrUpdate("SelvaPlus_notify",
                        "default",
                        () => selva.Handler(),
                        SelvaPlus.CronExpression,
                        new RecurringJobOptions
                        {
                            TimeZone = _timeZone,
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists("SelvaPlus_notify");
                }
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}

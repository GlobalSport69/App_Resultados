using Hangfire;
using LotteryResult.Services.PremierPlussJobs;
using LotteryResult.Services.PremierPlussJobs.CloseNotification;
using Microsoft.AspNetCore.Mvc;

namespace LotteryResult.Controllers
{
    [Route("jobs")]
    public class CloseNotifyController : Controller
    {
        private Guacharo guacharo;
        private SelvaPlus selva;
        private SetLimitForIntegrations SetQuotas;
        private UpdateRates updateRates;

        public CloseNotifyController(Guacharo guacharo, SelvaPlus selva, SetLimitForIntegrations setQuotas, UpdateRates updateRates)
        {
            this.guacharo = guacharo;
            this.selva = selva;
            SetQuotas = setQuotas;
            this.updateRates = updateRates;
        }


        // GET: Close?product_id=1&status=true
        [HttpGet]
        public ActionResult EnableNotifyClose([FromQuery] string job_key, [FromQuery] bool status = true)
        {
            var _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");
            if (job_key == "job_guacharo_preaward")
            {
                if (status)
                {
                    RecurringJob.AddOrUpdate("job_guacharo_preaward",
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
                    RecurringJob.RemoveIfExists("job_guacharo_preaward");
                }
            }

            if (job_key == "job_selvaplus_preaward")
            {
                if (status)
                {
                    RecurringJob.AddOrUpdate("job_selvaplus_preaward",
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
                    RecurringJob.RemoveIfExists("job_selvaplus_preaward");
                }
            }

            if (job_key == "job_setlimit")
            {
                if (status)
                {
                    RecurringJob.AddOrUpdate("job_setlimit",
                        "default",
                        () => SetQuotas.Handler(),
                        SetLimitForIntegrations.CronExpression,
                        new RecurringJobOptions
                        {
                            TimeZone = _timeZone,
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists("job_setlimit");
                }
            }
            
            if (job_key == "job_updateRate")
            {
                if (status)
                {
                    RecurringJob.AddOrUpdate("job_updateRate",
                        "default",
                        () => updateRates.Handler(),
                        UpdateRates.CronExpression,
                        new RecurringJobOptions
                        {
                            TimeZone = _timeZone,
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists("job_updateRate");
                }
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}

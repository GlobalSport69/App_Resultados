using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LotteryResult.Controllers
{
    [Authorize]
    public class ResultsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public ResultsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // GET: ResultsController
        public async Task<ActionResult> Index()
        {
            // Obtén la zona horaria de Venezuela
            TimeZoneInfo venezuelaZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");

            // Obtén la fecha y hora actual en UTC
            DateTime utcNow = DateTime.UtcNow;

            // Convierte la fecha y hora actual a la zona horaria de Venezuela
            DateTime venezuelaNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, venezuelaZone);
            //var today = DateTime.Now.ToUniversalTime().Date;
            var today = venezuelaNow.ToUniversalTime().Date;
            var products = await unitOfWork.ProductRepository.GetResultByProductsByDate(today);

            ViewBag.Products = products.OrderBy(x => x.Id).ToList();

            return View();
        }
    }
}

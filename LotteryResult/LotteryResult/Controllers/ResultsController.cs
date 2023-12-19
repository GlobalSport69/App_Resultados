using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LotteryResult.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IProductRepository productRepository;

        public ResultsController(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        // GET: ResultsController
        public async Task<ActionResult> Index()
        {
            var today = DateTime.Now.ToUniversalTime().Date;
            var products = await productRepository.GetResultByProductsByDate(today);

            ViewBag.Products = products.OrderBy(x => x.Id).ToList();

            return View();
        }
    }
}

﻿using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using LotteryResult.Extensions;

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
        public async Task<ActionResult> Index(string date = null)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime venezuelaNow = utcNow.ToVenezuelaTimeZone();
            if (!string.IsNullOrEmpty(date))
            {
                venezuelaNow = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            var today = venezuelaNow.ToUniversalTime().Date;

            var products = await unitOfWork.ProductRepository.GetResultByProductsByDate(today);

            ViewBag.Products = products.OrderBy(x => x.Id).ToList();
            ViewBag.Date = venezuelaNow.Date;

            return View();
        }
    }
}

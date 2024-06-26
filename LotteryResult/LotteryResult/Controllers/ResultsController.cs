﻿using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using LotteryResult.Extensions;
using LotteryResult.Services;
using Microsoft.Extensions.Logging;

namespace LotteryResult.Controllers
{
    [Authorize]
    public class ResultsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ResultsController> logger;

        public ResultsController(IUnitOfWork unitOfWork, ILogger<ResultsController> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        // GET: ResultsController
        public async Task<ActionResult> Index(string date = null)
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;
                if (!string.IsNullOrEmpty(date))
                {
                    venezuelaNow = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                logger.LogInformation("================================================================");
                logger.LogInformation(venezuelaNow.ToString("yyyy-MM-dd hh:mm:ss"));
                logger.LogInformation("================================================================");
                var products = await unitOfWork.ProductRepository.GetResultByProductsByDate(venezuelaNow);
            
                ViewBag.Products = products.OrderBy(x => x.Id).ToList();
                ViewBag.Date = venezuelaNow.Date;

                return View();
            }
            catch (Exception ex) 
            {
                logger.LogError(ex.Message, ex);
                throw;
            }
        }
    }
}

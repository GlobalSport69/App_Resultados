﻿using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleChanceOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 17;
        private const int providerID = 17;
        private readonly ILogger<TripleChanceOfficial> _logger;
        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "01:00 PM", 253 },
            { "04:30 PM", 256 },
            { "07:00 PM", 259 },
        };

        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "01:00 PM", 254 },
            { "04:30 PM", 257 },
            { "07:00 PM", 260 },
        };
        public TripleChanceOfficial(IUnitOfWork unitOfWork, ILogger<TripleChanceOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions
                    {
                        Headless = true,
                        Args = new string[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox"
                        }
                    });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://tuchance.com.ve/resultadosChance", waitUntil: WaitUntilNavigation.Networkidle2);
                //await page.WaitForNetworkIdleAsync();
                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.table tbody tr')]
                    .flatMap(x => {
                        let [t, a, b] = [...x.querySelectorAll('td span')]
                        return [
                            {
                                time: t.innerText,
                                result: a.innerText,
                                sorteo: 'Triple A'
                            },
                            {
                                time: t.innerText,
                                result: b.innerText,
                                sorteo: 'Triple B'
                            }
                        ]
                    })

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleChanceOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.Substring(0, item.Time.Length - 2) + " " + item.Time.Substring(item.Time.Length - 2);
                    time = time.ToUpper();

                    var premierId = item.Sorteo == "Triple A" ? TripleA[time] : TripleB[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = item.Sorteo,
                        PremierId = premierId,
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();

                var needSave = false;
                // no hay resultado nuevo
                var len = oldResult.Count();
                if (len == newResult.Count())
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (oldResult[i].Time == newResult[i].Time && oldResult[i].Result1 != newResult[i].Result1)
                        {
                            oldResult[i].Result1 = newResult[i].Result1;
                            unitOfWork.ResultRepository.Update(oldResult[i]);
                            needSave = true;
                        }
                    }
                }

                // hay resultado nuevo
                if (newResult.Count() > len)
                {
                    var founds = newResult.Where(x => !oldResult.Any(y => y.Time == x.Time));

                    foreach (var item in founds)
                    {
                        unitOfWork.ResultRepository.Insert(item);
                        needSave = true;
                    }
                }

                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleZuliaOfficial));
                    return;
                }
                //var oldResult = await unitOfWork.ResultRepository
                //    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                //foreach (var item in oldResult)
                //{
                //    unitOfWork.ResultRepository.Delete(item);
                //}

                //foreach (var item in someObject)
                //{
                //    var newTime = item.Time.Substring(0, item.Time.Length - 2) + " " + item.Time.Substring(item.Time.Length - 2);
                //    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                //    {
                //        Result1 = item.Result,
                //        Time = newTime,
                //        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                //        ProductId = productID,
                //        ProviderId = providerID,
                //        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                //        Sorteo = item.Sorteo
                //    });
                //}


                //await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleChanceOfficial));
                throw;
            }
        }
    }
}


using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class AstroZamoranoOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 22;
        private const int providerID = 22;
        private readonly ILogger<AstroZamoranoOfficial> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "12:00 PM", 131 },
            { "04:00 PM", 160 },
            { "07:00 PM", 133 }
        };
        public AstroZamoranoOfficial(IUnitOfWork unitOfWork, ILogger<AstroZamoranoOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                var venezuelaNow = DateTime.Now;

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
                //_logger.LogInformation("///////////////////////////////////");
                //_logger.LogInformation(DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss"));
                //_logger.LogInformation(venezuelaNow.ToString("yyyy-MM-dd hh:mm:ss"));
                //_logger.LogInformation(venezuelaNow.Date.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss"));
                //_logger.LogInformation("///////////////////////////////////");

                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("http://triplezamorano.com/action/index", waitUntil:WaitUntilNavigation.Networkidle2);

                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('table tr td');
                    return tds.length > 1;
                }", new WaitForFunctionOptions
                {
                    PollingInterval = 1000,
                });

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    var fechaFormateada = date;

                    let table = document.querySelector('table');
                    var result = [...table.querySelector('tbody').querySelectorAll('tr')].filter(x => ([...x.querySelectorAll('td')][1]).innerText == fechaFormateada)
                    let r = result.map(x => {
                        let [,,,time,, tsigno] = x.querySelectorAll('td');
                        return {
                            time: time.innerText,
                            result: tsigno.innerText
                        }
                    })

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(AstroZamoranoOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var premierId = lotteries[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(AstroZamoranoOfficial));
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
                //    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                //    {
                //        Result1 = item.Result,
                //        Time = item.Time,
                //        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                //        ProductId = productID,
                //        ProviderId = providerID,
                //        ProductTypeId = (int)ProductTypeEnum.ZODIACAL
                //    });
                //}

                //Console.WriteLine(someObject);

                //await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(AstroZamoranoOfficial));
                throw;
            }
        }
    }
}
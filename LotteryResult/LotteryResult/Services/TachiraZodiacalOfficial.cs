using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;
using System.Globalization;

namespace LotteryResult.Services
{
    public class TachiraZodiacalOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 20;
        private const int providerID = 20;
        private readonly ILogger<TachiraZodiacalOfficial> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "01:15 PM", 141 },
            { "04:45 PM", 143 },
            { "10:00 PM", 145 }
        };
        public TachiraZodiacalOfficial(IUnitOfWork unitOfWork, ILogger<TachiraZodiacalOfficial> logger)
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
                await page.GoToAsync("https://tripletachira.com", waitUntil: WaitUntilNavigation.Networkidle2);

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.single-ticket')]
                    .map(x => {
                        let title = x.querySelector('.ticket-rate').innerText;
                        if (title == '') return null;
                        let time = title.substring(title.length - 7, title.length - 2).trim() +' '+ title.substr(title.length - 2);
                        return ({
                            time: time, 
                            result: x.querySelector('.ticket-name')?.innerText,
                            sorteo:  title.substring(0, 9).trim()
                        });
                    }).filter(x => x != null && x.sorteo == 'ZODIACAL')

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TachiraZodiacalOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {

                    string time = LaGranjitaTerminalOfficial.FormatTime(item.Time).ToUpper();
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TachiraZodiacalOfficial));
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
                //        Time = LaGranjitaTerminalOfficial.FormatTime(item.Time),
                //        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                //        ProductId = productID,
                //        ProviderId = providerID,
                //        ProductTypeId = (int)ProductTypeEnum.ZODIACAL
                //    });
                //}

                //await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TachiraZodiacalOfficial));
                throw;
            }
        }
    }
}

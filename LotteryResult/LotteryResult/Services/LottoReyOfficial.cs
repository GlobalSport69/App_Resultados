using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LottoReyOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 5;
        private const int providerID = 4;
        private readonly ILogger<LottoReyOfficial> _logger;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "08:30 AM", 289 },
            { "09:30 AM", 67 },
            { "10:30 AM", 68 },
            { "11:30 AM", 69 },
            { "12:30 PM", 70 },
            { "01:30 PM", 71 },
            { "02:30 PM", 72 },
            { "03:30 PM", 73 },
            { "04:30 PM", 74 },
            { "05:30 PM", 75 },
            { "06:30 PM", 76 },
            { "07:30 PM", 77 }
        };
        public LottoReyOfficial(IUnitOfWork unitOfWork, ILogger<LottoReyOfficial> logger)
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
                await page.GoToAsync("https://lottorey.com.ve/"+ venezuelaNow.ToString("yyyy/MM/dd"));

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let fechaFormateada = date;
                    let r = [...document.querySelectorAll('#main-container .card')]
                    .filter(x => x.querySelector('.texto-fecha').innerText == fechaFormateada)
                    .map(x => ({
                        time: x.querySelector('.texto-hora').innerText,
                        result: x.querySelector('.card-footer').innerText,
                    }))

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoReyOfficial));
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
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaGranjitaOfficial));
                    return;
                }

                //var oldResult = await unitOfWork.ResultRepository
                //    .GetAllByAsync(x => x.ProviderId == lottoReyProviderID && x.CreatedAt.Date == venezuelaNow.Date);

                //foreach (var item in oldResult)
                //{
                //    unitOfWork.ResultRepository.Delete(item);
                //}

                //foreach (var item in someObject)
                //{
                //    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                //    {
                //        Result1 = item.Result,
                //        Time = item.Time.ToUpper(),
                //        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                //        ProductId = lottoReyID,
                //        ProviderId = lottoReyProviderID,
                //        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS
                //    });
                //}

                //Console.WriteLine(someObject);

                //await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception:ex, message: nameof(LottoReyOfficial));
                throw;
            }
        }
    }
}

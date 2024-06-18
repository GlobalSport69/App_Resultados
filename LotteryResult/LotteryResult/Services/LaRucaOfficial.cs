using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaRucaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 9;
        private const int providerID = 8;
        private readonly ILogger<LaRucaOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:15 AM", 102 },
            { "10:15 AM", 103 },
            { "11:15 AM", 104 },
            { "12:15 PM", 105 },
            { "01:15 PM", 106 },
            { "02:15 PM", 226 },
            { "03:15 PM", 107 },
            { "04:15 PM", 108 },
            { "05:15 PM", 109 },
            { "06:15 PM", 110 },
            { "07:15 PM", 111 }
        };

        public LaRucaOfficial(IUnitOfWork unitOfWork, ILogger<LaRucaOfficial> logger, INotifyPremierService notifyPremierService)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
            this.notifyPremierService = notifyPremierService;
        }

        public async Task Handler()
        {
            try
            {
                //using var browserFetcher = new BrowserFetcher();
                //await browserFetcher.DownloadAsync();
                //await using var browser = await Puppeteer.LaunchAsync(
                //    new LaunchOptions { Headless = true });
                //await using var page = await browser.NewPageAsync();
                //await page.GoToAsync("https://lottollano.com.ve/laruca.php");

                //var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                //    let fecha = new Date();
                //    let dia = String(fecha.getDate()).padStart(2, '0');
                //    let mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Los meses en JavaScript empiezan desde 0
                //    let ano = fecha.getFullYear();
                //    let fechaFormateada = dia + '/' + mes + '/' + ano;

                //    let r = [...document.querySelectorAll('table tbody tr')]
                //    .filter(tr => ([...tr.querySelectorAll('td')][0]).innerHTML.split('<br>')[1] == fechaFormateada)
                //    .map(tr => ({
                //        time: ([...tr.querySelectorAll('td')][0]).innerHTML.split('<br>')[2],
                //        result: ([...tr.querySelectorAll('td')][1]).querySelector('img').getAttribute('src').replace('./laruca/', '').replace('.jpg', '')
                //    }))

                //    return r;
                //}");
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
                await page.GoToAsync("https://triples.bet/products-results/la-ruca-results");

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.results-content-item')]
                    .map(x => ({
                        time: x.querySelector('.results-title-draw-hour').innerText,
                        result: x.querySelector('.number-ruca')?.innerText
                    })).filter(x => x.result !== undefined);

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaRucaOfficial));
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
                        ProductTypeId = (int)ProductTypeEnum.TERMINALES,
                        PremierId = premierId,
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();


                var toUpdate = new List<Result>();
                foreach (var item in newResult)
                {
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Result1 != y.Result1);
                    if (found is null)
                        continue;

                    found.Result1 = item.Result1;
                    toUpdate.Add(found);
                }
                var toInsert = newResult.Where(x => !oldResult.Exists(y => x.Time == y.Time));
                var needSave = false;
                foreach (var item in toUpdate)
                {
                    unitOfWork.ResultRepository.Update(item);
                    needSave = true;
                }
                foreach (var item in toInsert)
                {
                    unitOfWork.ResultRepository.Insert(item);
                    needSave = true;
                }


                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();

                    if (toUpdate.Any())
                        notifyPremierService.Handler(toUpdate.Select(x => x.Id).ToList(), NotifyType.Update);
                    if (toInsert.Any())
                        notifyPremierService.Handler(toInsert.Select(x => x.Id).ToList(), NotifyType.New);
                }



                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaRucaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaRucaOfficial));
                throw;
            }
        }
    }
}

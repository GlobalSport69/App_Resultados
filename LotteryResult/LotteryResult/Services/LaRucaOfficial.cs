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

        private Dictionary<string, string> HoursMap = new Dictionary<string, string>
        {
            { "9:15 AM", "09:15 AM" },
            { "10:15 AM", "10:15 AM" },
            { "11:15 AM", "11:15 AM" },
            { "12:15 PM", "12:15 PM" },
            { "13:15 PM", "01:15 PM" },
            { "14:15 PM", "02:15 PM" },
            { "15:15 PM", "03:15 PM" },
            { "16:15 PM", "04:15 PM" },
            { "17:15 PM", "05:15 PM" },
            { "18:15 PM", "06:15 PM" },
            { "19:15 PM", "07:15 PM" },
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
                await page.GoToAsync("https://www.laruca.com.ve", waitUntil: WaitUntilNavigation.Networkidle2);

                // Espera hasta que haya al menos 1 elementos 'div' dentro de '.resultados'
                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('.resultados > div');
                    return tds.length > 0;
                }", new WaitForFunctionOptions
                {
                    PollingInterval = 1000,
                });

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.resultados .card-result')].map(x => ({
                        time: x.querySelector('h4').innerText.replace('SORTEO DE ', ''),
                        result: x.querySelector('img').src.replace('https://latococa.com/assets/fichas/laruca/', '').replace('.png', '')
                    }));

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
                    var time = HoursMap[item.Time.ToUpper()];
                    var premierId = lotteries[time];

                    return new Result
                    {
                        Result1 = item.Result.PadLeft(2, '0'),
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
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Sorteo == y.Sorteo && item.Result1 != y.Result1);
                    if (found is null)
                        continue;

                    found.Result1 = item.Result1;
                    toUpdate.Add(found);
                }
                var toInsert = newResult.Where(x => !oldResult.Exists(y => x.Time == y.Time && x.Sorteo == y.Sorteo));
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

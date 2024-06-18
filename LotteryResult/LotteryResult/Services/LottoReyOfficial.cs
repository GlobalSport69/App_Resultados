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
        private INotifyPremierService notifyPremierService;

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
        public LottoReyOfficial(IUnitOfWork unitOfWork, ILogger<LottoReyOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync("https://lottorey.com.ve/" + venezuelaNow.ToString("yyyy/MM/dd"));

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('#main-container .card')]
                    .map(x => ({
                        time: x.querySelector('.texto-hora').innerText,
                        result: x.querySelector('.card-footer').innerText,
                    }))

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoReyOfficial));
                    return;
                }

                await page.GoToAsync("https://lottorey.com.ve/" + venezuelaNow.ToString("yyyy/MM/dd")+ "/page/2");
                var responsePage2 = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('#main-container .card')]
                    .map(x => ({
                        time: x.querySelector('.texto-hora').innerText,
                        result: x.querySelector('.card-footer').innerText,
                    }))

                    return r;
                }");

                response.AddRange(responsePage2);

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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LottoReyOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception:ex, message: nameof(LottoReyOfficial));
                throw;
            }
        }
    }
}

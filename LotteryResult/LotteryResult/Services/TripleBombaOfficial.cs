using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;
using System.Globalization;

namespace LotteryResult.Services
{
    public class TripleBombaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 15;
        private const int providerID = 15;
        private readonly ILogger<TripleBombaOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "01:30 PM", 218 },
            { "04:30 PM", 220 },
            { "08:30 PM", 222 }
        };

        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "01:30 PM", 219 },
            { "04:30 PM", 221 },
            { "08:30 PM", 223 }
        };
        public TripleBombaOfficial(IUnitOfWork unitOfWork, ILogger<TripleBombaOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync("https://www.triplebomba.com/sistema", waitUntil: WaitUntilNavigation.Networkidle2);

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let dateText = document.querySelector('legend > h6').innerText;
                    if(dateText.substr(dateText.length - 10) != date) return [];

                    let r = [...document.querySelector('.card-deck').querySelectorAll('.card')]
                    .flatMap(x => [...x.querySelectorAll('.card-body .mx-auto div')]
                                .map(y => ({time: y.querySelector('h6').innerText.split('-')[1].trim(),
                                    result: y.querySelector('input').value,
                                    sorteo: y.querySelector('h6').innerText.split('-')[0].trim()
                                }))
                    ).filter(x => x.result !== '---')

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleBombaOfficial));
                    return;
                }
                var oldResult = await unitOfWork.ResultRepository.GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    string time = LaGranjitaTerminalOfficial.FormatTime(item.Time.ToUpper());
                    var premierId = item.Sorteo == "Triple A" ? TripleA[time] : TripleB[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        Sorteo = item.Sorteo,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleBombaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleBombaOfficial));
                throw;
            }
        }
    }
}

using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class UnelotonOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 38;
        private const int providerID = 37;
        private readonly ILogger<UnelotonOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> LotteriesA = new Dictionary<string, long>
        {
            { "01:15 PM", 316 },
            { "04:15 PM", 319 },
            { "07:10 PM", 320 }
        };

        private Dictionary<string, long> LotteriesB = new Dictionary<string, long>
        {
            { "01:15 PM", 317 },
            { "04:15 PM", 319 },
            { "07:10 PM", 321 }
        };
        public UnelotonOfficial(IUnitOfWork unitOfWork, ILogger<UnelotonOfficial> logger, INotifyPremierService notifyPremierService)
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
                var date = DateTime.Now.ToString("yyyy-MM-dd");

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
                await page.GoToAsync($"https://loteriadehoy.com/animalito/cazaloton/resultados/", waitUntil: WaitUntilNavigation.Networkidle2);

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.resultados tbody tr')].flatMap(x => {
                                let [time, a, b] = x.querySelectorAll('td');

                                return [
                                    {
                                        time: time.textContent.trim(), 
                                        result: a?.textContent.replace('A :', '').trim(),
                                        sorteo: 'Triple A' 
                                    },
                                    {
                                        time: time.textContent.trim(), 
                                        result: b?.textContent.replace('B :', '').trim(),
                                        sorteo: 'Triple B' 
                                    }
                                ]
    
                            }).filter(x => x.result != undefined);
                    return r;
                }");


                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(UnelotonOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var premierId = item.Sorteo == "Triple A" ? LotteriesA[time] : LotteriesB[time];
                    var number = item.Result;

                    return new Result
                    {
                        Result1 = number,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        PremierId = premierId,
                        Sorteo = item.Sorteo,
                        //Number: item.Result,
                        //Complement: complement
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(UnelotonOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(UnelotonOfficial));
                throw;
            }
        }
    }
}

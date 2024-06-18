using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleTachiraOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 19;
        private const int providerID = 19;
        private readonly ILogger<TripleTachiraOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "01:15 PM", 121 },
            { "04:45 PM", 122 },
            { "10:00 PM", 123 },
        };
        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "01:15 PM", 140 },
            { "04:45 PM", 142 },
            { "10:00 PM", 144 },
        };

        private Dictionary<string, long> TripleC = new Dictionary<string, long>
        {
            { "01:15 PM", 141 },
            { "04:45 PM", 143 },
            { "10:00 PM", 145 }
        };
        public TripleTachiraOfficial(IUnitOfWork unitOfWork, ILogger<TripleTachiraOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync("https://tripletachira.com", waitUntil: WaitUntilNavigation.Networkidle2);

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.single-ticket')]
                        .map(x => {
                            let title = x.querySelector('.ticket-rate').innerText;
                            if (title == '') return null;
                            let time = title.substring(title.length - 7, title.length - 2).trim() +' '+ title.substr(title.length - 2);
                            let result = x.querySelector('.ticket-name')?.innerText;
                            let dic = { 'TACHIRA A':'Triple A','TACHIRA B':'Triple B', 'ZODIACAL': 'Tachira Zodiacal' }

                            return ({
                                time: time, 
                                result: result.slice(0, 3),
                                sorteo:  dic[title.substring(0, 9).trim()],
                                complement: result.includes('\n') ? result.slice(4, 9) : null
                            });
                        }).filter(x => x != null);

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleTachiraOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = LaGranjitaTerminalOfficial.FormatTime(item.Time);
                    long premierId = 0;
                    if (item.Sorteo == "Triple A")
                        premierId = TripleA[time];

                    if (item.Sorteo == "Triple B")
                        premierId = TripleB[time];

                    if (item.Sorteo == "Tachira Zodiacal")
                        premierId = TripleC[time];

                    var complement = string.IsNullOrEmpty(item.Complement) ? null : $" {TripleCaracasOfficial.ZodiacSigns[item.Complement]}";
                    var resultado = item.Result + complement ?? "";

                    return new Result
                    {
                        Result1 = resultado,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = item.Sorteo,
                        PremierId = premierId,
                        //Number: item.Result,
                        //Complement: item.Complement
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleTachiraOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleTachiraOfficial));
                throw;
            }
        }
    }
}

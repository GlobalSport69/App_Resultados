using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Models.CarruselMillonario;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class MegaAnimalOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 32;
        private const int providerID = 31;
        private readonly ILogger<MegaAnimalOfficial> _logger;
        private readonly INotifyPremierService notifyPremierService;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:00 AM", 293 },
            { "10:00 AM", 294 },
            { "11:00 AM", 295 },
            { "12:00 PM", 296 },
            { "01:00 PM", 297 },
            { "02:00 PM", 298 },
            { "03:00 PM", 299 },
            { "04:00 PM", 300 },
            { "05:00 PM", 301 },
            { "06:00 PM", 302 },
            { "07:00 PM", 303 },
            { "08:00 PM", 304 },
        };

        public MegaAnimalOfficial(IUnitOfWork unitOfWork, ILogger<MegaAnimalOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync("https://megaanimal40.com/resultados/mega_animal40/"+ venezuelaNow.ToString("yyyy-MM-dd")+"/", 
                    waitUntil: WaitUntilNavigation.Networkidle2);

                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('#ultimos_resultados div');
                    return tds.length > 0;
                }", new WaitForFunctionOptions
                {
                    PollingInterval = 1000,
                });

                await page.EvaluateFunctionAsync(@"() => {
                    window.scrollTo(0, document.body.scrollHeight);
                }");

                await Task.Delay(500);

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('#ultimos_resultados .single_portfolio_content1')]
                    .map(x => {
                        let [_, hourLabel] = x.querySelectorAll('h6')
                        let result = x.querySelector('.badge b').innerText
                        let isMega = !!x.querySelector('.single_blog_date_inner');
                        return {
                            time: hourLabel.innerText,
                            result: result,
                            complement: isMega,
                        }
                    })

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(MegaAnimalOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item =>
                {
                    var time = item.Time.Trim().ToUpper();
                    var number = new String(item.Result.TakeWhile(c => c != ' ').ToArray());
                    var animal = item.Result.Substring(number.Length, item.Result.Length - number.Length);
                    var IsMega = bool.Parse(item.Complement);
                    var premierId = lotteries[time];

                    var resultado = number + " " + animal.Capitalize() + (IsMega ? " (MEGA)" : "");

                    return new Result
                    {
                        Result1 = resultado,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = premierId,
                        //number = number,
                        //animal = animal
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(MegaAnimalOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(MegaAnimalOfficial));
                throw;
            }
        }
    }
}

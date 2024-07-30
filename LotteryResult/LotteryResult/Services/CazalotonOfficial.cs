using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class CazalotonOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 33;
        private const int providerID = 32;
        private readonly ILogger<CazalotonOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> Lotteries = new Dictionary<string, long>
        {
            { "09:00 AM", 305 },
            { "10:00 AM", 306 },
            { "11:00 AM", 307 },
            { "12:00 PM", 308 },
            { "01:00 PM", 309 },
            { "02:00 PM", 310 },
            { "03:00 PM", 311 },
            { "04:00 PM", 312 },
            { "05:00 PM", 313 },
            { "06:00 PM", 314 },
            { "07:00 PM", 315 }
        };
        public CazalotonOfficial(IUnitOfWork unitOfWork, ILogger<CazalotonOfficial> logger, INotifyPremierService notifyPremierService)
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
                    let r = [...document.querySelectorAll('.js-con > div')]
                    .map(x => ({ 
                        time: x.querySelector('h5').textContent.replace('Cazaloton', '').trim(), 
                        result: x.querySelector('h4').textContent 
                    }));
                    return r;
                }");


                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(CazalotonOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var premierId = Lotteries[time];
                    var number = new String(item.Result.TakeWhile(c => c != ' ').ToArray());
                    var animal = item.Result.Substring(number.Length, item.Result.Length - number.Length).Trim();
                    number = number == "0" || number == "00" ? number : number.PadLeft(2, '0');

                    return new Result
                    {
                        Result1 = $"{number} {animal.Capitalize()}",
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = premierId,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(CazalotonOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(CazalotonOfficial));
                throw;
            }
        }
    }
}

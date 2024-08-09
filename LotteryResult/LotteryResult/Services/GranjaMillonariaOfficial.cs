using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class GranjaMillonariaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 34;
        private const int providerID = 33;
        private readonly ILogger<GranjaMillonariaOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        public GranjaMillonariaOfficial(IUnitOfWork unitOfWork, ILogger<GranjaMillonariaOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync($"http://www.granjamillonaria.com/#!/hoy", waitUntil: WaitUntilNavigation.Networkidle2);

                // Espera hasta que haya al menos 1 elementos 'div' dentro de '#resultados'
                //await page.WaitForFunctionAsync(@"() => {
                //    const tds = document.querySelectorAll('#resultados > div');
                //    return tds.length > 0;
                //}", new WaitForFunctionOptions
                //{
                //    PollingInterval = 1000,
                //});

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.panel-animalito .row > div')]
                    .map(x => ({
                        time : x.querySelector('h2').textContent,
                        result : x.querySelector('img').src
                            .replace('http://www.granjamillonaria.com/img/', '')
                            .replace('v1/', '').replace('.png', '')
                    }))
                    .filter(r => r.result != 'esperando.jpg');

                    return r;
                }");


                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(GranjaMillonariaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var number = new String(item.Result.TakeWhile(c => char.IsNumber(c)).ToArray());
                    var animal = item.Result.Substring(number.Length, item.Result.Length - number.Length).Trim().Capitalize();
                    //var premierId = Lotteries[time];

                    return new Result
                    {
                        Result1 = $"{number} {animal}",
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        //PremierId = premierId,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(GranjaMillonariaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(GranjaMillonariaOfficial));
                throw;
            }
        }
    }
}


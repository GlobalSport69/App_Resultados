using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LotteryResult.Services
{
    public class TripleChanceOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 17;
        private const int providerID = 17;
        private readonly ILogger<TripleChanceOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "01:00 PM", 253 },
            { "04:30 PM", 256 },
            { "07:00 PM", 259 },
        };

        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "01:00 PM", 254 },
            { "04:30 PM", 257 },
            { "07:00 PM", 260 },
        };

        private Dictionary<string, long> TripleC = new Dictionary<string, long>
        {
            { "01:00 PM", 255 },
            { "04:30 PM", 258 },
            { "07:00 PM", 261 }
        };
        public TripleChanceOfficial(IUnitOfWork unitOfWork, ILogger<TripleChanceOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync("https://tuchance.com.ve/resultadosChance", waitUntil: WaitUntilNavigation.Networkidle2);

                # region Para probar con resultados de dias anteriores
                //await page.EvaluateFunctionAsync(@"(date) => {
                //    var found = [...document.querySelectorAll('li')].find(x => x.innerText == date);
                //    if(found === undefined){
                //        return;
                //    }
                //    found.click();
                //}", "11-6-2024" /* 6-6-2024 */);

                //await page.WaitForFunctionAsync(@"() => {
                //    const tds = document.querySelectorAll('table tr td');
                //    return tds.length > 1;
                //}", new WaitForFunctionOptions
                //{
                //    PollingInterval = 1000,
                //});
                #endregion

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.table tbody tr')]
                    .flatMap(x => {
                        let [t, a, b, c1, c2] = [...x.querySelectorAll('td span')]
                        return [
                            {
                                time: t.innerText,
                                result: a.innerText,
                                sorteo: 'Triple A',
                            },
                            {
                                time: t.innerText,
                                result: b.innerText,
                                sorteo: 'Triple B',
                            },
                            {
                                time: t.innerText,
                                result: c1.innerText,
                                sorteo: 'Chance Astral',
                                number: c1.innerText,
                                complement: c2.innerText
                            }
                        ]
                    })

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleChanceOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.Substring(0, item.Time.Length - 2) + " " + item.Time.Substring(item.Time.Length - 2);
                    time = time.ToUpper();

                    long premierId = 0;
                    if (item.Sorteo == "Triple A")
                        premierId = TripleA[time];

                    if (item.Sorteo == "Triple B")
                        premierId = TripleB[time];

                    if (item.Sorteo == "Chance Astral")
                        premierId = TripleC[time];

                    var complement = string.IsNullOrEmpty(item.Complement) ? null : $" {item.Complement}";
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
                        //Complement: complement
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleZuliaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleChanceOfficial));
                throw;
            }
        }
    }
}


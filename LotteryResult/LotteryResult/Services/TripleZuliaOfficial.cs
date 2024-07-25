using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleZuliaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 6;
        private const int providerID = 5;
        private readonly ILogger<TripleZuliaOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "12:45 PM", 112 },
            { "04:45 PM", 156 },
            { "07:05 PM", 114 },
        };

        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "12:45 PM", 124 },
            { "04:45 PM", 157 },
            { "07:05 PM", 128 },
        };

        private Dictionary<string, long> TripleC = new Dictionary<string, long>
        {
            { "12:45 PM", 148 },
            { "04:45 PM", 166 },
            { "07:05 PM", 151 }
        };

        public TripleZuliaOfficial(IUnitOfWork unitOfWork, ILogger<TripleZuliaOfficial> logger, INotifyPremierService notifyPremierService)
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
                await page.GoToAsync("https://resultadostriplezulia.com/", waitUntil: WaitUntilNavigation.Networkidle2);

                // Espera hasta que haya al menos 2 elementos 'td' dentro de un 'tr' en una tabla
                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('table tr td');
                    return tds.length > 1;
                }", new WaitForFunctionOptions
                {
                    PollingInterval = 1000,
                });

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let fechaFormateada = date;
                    let table = document.querySelector('table');
                    let r = [...table.querySelectorAll('tbody tr')]
                    .filter(x => [...x.querySelectorAll('td')][1].innerText == fechaFormateada)
                    .flatMap(x => {

                        let [,,,time,a,b,c] = x.querySelectorAll('td');
                        let [cNumber, signo] = c.innerText.split(' ');
                        
                        return [
                            {
                                time: time.innerText,
                                result: a.innerText,
                                sorteo: 'Triple A',
                            },
                            {
                                time: time.innerText,
                                result: b.innerText,
                                sorteo: 'Triple B',
                            },
                            {
                                time: time.innerText,
                                result: cNumber,
                                sorteo: 'Zodiaco del Zulia',
                                complement: signo
                            }
                        ];
                    });

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));


                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleZuliaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    long premierId = 0;
                    if (item.Sorteo == "Triple A")
                    {
                        premierId = TripleA[time];
                    }

                    if (item.Sorteo == "Triple B")
                    {
                        premierId = TripleB[time];
                    }

                    if (item.Sorteo == "Zodiaco del Zulia")
                    {
                        premierId = TripleC[time];
                    }

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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleZuliaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleZuliaOfficial));
                throw;
            }
        }
    }
}

using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleCalienteOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 4;
        private const int providerID = 6;
        private readonly ILogger<TripleCalienteOfficial> _logger;
        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "01:00 PM", 118 },
            { "04:30 PM", 161 },
            { "07:10 PM", 120 }
        };

        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "01:00 PM", 134 },
            { "04:30 PM", 162 },
            { "07:10 PM", 138 }
        };

        private Dictionary<string, long> TripleC = new Dictionary<string, long>
        {
            { "01:00 PM", 135 },
            { "04:30 PM", 163 },
            { "07:10 PM", 139 },
        };
        public TripleCalienteOfficial(IUnitOfWork unitOfWork, ILogger<TripleCalienteOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
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
                await page.GoToAsync("https://triplecaliente.com", waitUntil: WaitUntilNavigation.Networkidle2);

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
                    .filter(x => {
                        let list = [...x.querySelectorAll('td')];
                        return list[1].innerText == fechaFormateada;
                    })
                    .flatMap(x => {
                        let [,,,time,a,b,c] = [...x.querySelectorAll('td')];
                        let [cNumber, cSigno] = c.innerText.split(' ');
                        return [
                            {
                                time: time.innerText,
                                result: a.innerText,
                                sorteo: 'Triple A'
                            },
                            {
                                time: time.innerText,
                                result: b.innerText,
                                sorteo: 'Triple B'
                            },
                            {
                                time: time.innerText,
                                result: cNumber,
                                sorteo: 'Signo Caliente',
                                complement: cSigno
                            }
                        ];
                    });

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleCalienteOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    long premierId = 0;
                    if (item.Sorteo == "Triple A")
                        premierId = TripleA[time];

                    if (item.Sorteo == "Triple B")
                        premierId = TripleB[time];

                    if (item.Sorteo == "Signo Caliente")
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

                var needSave = false;
                // no hay resultado nuevo
                var len = oldResult.Count();
                if (len == newResult.Count())
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (oldResult[i].Time == newResult[i].Time && oldResult[i].Result1 != newResult[i].Result1)
                        {
                            oldResult[i].Result1 = newResult[i].Result1;
                            unitOfWork.ResultRepository.Update(oldResult[i]);
                            needSave = true;
                        }
                    }
                }

                // hay resultado nuevo
                if (newResult.Count() > len)
                {
                    var founds = newResult.Where(x => !oldResult.Any(y => y.Time == x.Time));

                    foreach (var item in founds)
                    {
                        unitOfWork.ResultRepository.Insert(item);
                        needSave = true;
                    }
                }

                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleCalienteOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleCalienteOfficial));
                throw;
            }
        }
    }
}

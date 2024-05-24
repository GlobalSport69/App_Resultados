using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleZamoranoOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 2;
        private const int providerID = 1;
        private readonly ILogger<TripleZamoranoOfficial> _logger;

        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "12:00 PM", 115 },
            { "07:00 PM", 117 },
            { "04:00 PM", 159 },
        };
        public TripleZamoranoOfficial(IUnitOfWork unitOfWork, ILogger<TripleZamoranoOfficial> logger)
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
                    new LaunchOptions {
                        Headless = true,
                        Args = new string[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox"
                        }
                    });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("http://triplezamorano.com/action/index", waitUntil: WaitUntilNavigation.Networkidle2);

                // Espera hasta que haya al menos 2 elementos 'td' dentro de un 'tr' en una tabla
                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('table tr td');
                    return tds.length > 1;
                }", new WaitForFunctionOptions 
                { 
                    PollingInterval = 1000,
                });

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    var fechaFormateada = date;
                    let table = document.querySelector('table');
                    var result = [...table.querySelector('tbody').querySelectorAll('tr')].filter(x => ([...x.querySelectorAll('td')][1]).innerText == fechaFormateada)
                    let r = result.map(x => ({
                        result: ([...x.querySelectorAll('td')][4]).innerText,
                        time: ([...x.querySelectorAll('td')][3]).innerText
                    }))

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleZamoranoOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var premierId = TripleA[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = item.Time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        PremierId = premierId,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleZamoranoOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleZamoranoOfficial));
                throw;
            }
        }
    }
}

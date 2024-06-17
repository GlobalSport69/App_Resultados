using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class ElRucoTriplesBet : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 8;
        private const int providerID = 7;
        private readonly ILogger<ElRucoTriplesBet> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "08:00 AM", 184 },
            { "09:00 AM", 185 },
            { "10:00 AM", 186 },
            { "11:00 AM", 187 },
            { "12:00 PM", 188 },
            { "01:00 PM", 189 },
            { "02:00 PM", 190 },
            { "03:00 PM", 191 },
            { "04:00 PM", 192 },
            { "05:00 PM", 193 },
            { "06:00 PM", 194 },
            { "07:00 PM", 195 }
        };
        public ElRucoTriplesBet(IUnitOfWork unitOfWork, ILogger<ElRucoTriplesBet> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                //using var browserFetcher = new BrowserFetcher();
                //await browserFetcher.DownloadAsync();
                //await using var browser = await Puppeteer.LaunchAsync(
                //    new LaunchOptions { Headless = true });
                //await using var page = await browser.NewPageAsync();
                //await page.GoToAsync("https://lottollano.com.ve/elruco.php");

                //var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                //    let fecha = new Date();
                //    let dia = String(fecha.getDate()).padStart(2, '0');
                //    let mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Los meses en JavaScript empiezan desde 0
                //    let ano = fecha.getFullYear();
                //    let fechaFormateada = dia + '/' + mes + '/' + ano;

                //    let r = [...document.querySelectorAll('table tbody tr')]
                //    .filter(tr => ([...tr.querySelectorAll('td')][0]).innerHTML.split('<br>')[1] == fechaFormateada)
                //    .map(tr => ({
                //        time: ([...tr.querySelectorAll('td')][0]).innerHTML.split('<br>')[2],
                //        result: ([...tr.querySelectorAll('td')][1]).querySelector('img').getAttribute('src').replace('./elruco/', '').replace('.jpg', '')
                //    }))

                //    return r;
                //}");

                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new string[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox"
                        }
                });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://triples.bet/products-results/resultados-el-ruco");

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.results-content-item')]
                    .map(x => ({
                        time: x.querySelector('.results-title-draw-hour').innerText,
                        result: x.querySelector('.number-ruco')?.innerText
                    })).filter(x => x.result !== undefined);

                    return r;
                }");

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoReyOfficial));
                    return;
                }

                var venezuelaNow = DateTime.Now;

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var premierId = lotteries[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        PremierId = premierId,
                        ProductTypeId = (int)ProductTypeEnum.TERMINALES
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(ZodiacoDelZuliaOfficial));
                    return;
                }

                //var oldResult = await unitOfWork.ResultRepository
                //    .GetAllByAsync(x => x.ProviderId == elRucoProviderID && x.CreatedAt.Date == DateTime.Now.Date);
                //foreach (var item in oldResult)
                //{
                //    unitOfWork.ResultRepository.Delete(item);
                //}

                //foreach (var item in someObject)
                //{
                //    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                //    {
                //        Result1 = item.Result,
                //        Time = item.Time.ToUpper(),
                //        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                //        ProductId = elRucoID,
                //        ProviderId =elRucoProviderID,
                //        ProductTypeId = (int)ProductTypeEnum.TERMINALES
                //    });
                //}

                //await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LottoReyOfficial));
                throw;
            }
        }
    }
}

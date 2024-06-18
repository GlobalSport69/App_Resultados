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
        private INotifyPremierService notifyPremierService;

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
        public ElRucoTriplesBet(IUnitOfWork unitOfWork, ILogger<ElRucoTriplesBet> logger, INotifyPremierService notifyPremierService)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
            this.notifyPremierService = notifyPremierService;
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(ElRucoTriplesBet));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ElRucoTriplesBet));
                throw;
            }
        }
    }
}

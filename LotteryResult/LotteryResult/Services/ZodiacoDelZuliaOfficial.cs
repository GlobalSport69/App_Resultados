using Azure;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class ZodiacoDelZuliaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 24;
        private const int providerID = 24;
        private readonly ILogger<ZodiacoDelZuliaOfficial> _logger;
        private Dictionary<string, long> TripleC = new Dictionary<string, long>
        {
            { "12:45 PM", 125 },
            { "04:45 PM", 158 },
            { "07:05 PM", 129 }
        };
        public ZodiacoDelZuliaOfficial(IUnitOfWork unitOfWork, ILogger<ZodiacoDelZuliaOfficial> logger)
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
                await page.GoToAsync("http://www.resultadostriplezulia.com/action/index", waitUntil: WaitUntilNavigation.Networkidle2);

                //var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                //    //let fecha = new Date();
                //    //let dia = String(fecha.getDate()).padStart(2, '0');
                //    //let mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Los meses en JavaScript empiezan desde 0
                //    //let ano = fecha.getFullYear();
                //    //let fechaFormateada = dia + '-' + mes + '-' + ano;
                //    let fechaFormateada = date;

                //    let iframe = document.querySelector('iframe')
                //    let contenidoDelIframe = iframe.contentDocument || iframe.contentWindow.document;
                //    let table = contenidoDelIframe.querySelector('#miTabla');

                //    let r = [...table.querySelectorAll('tbody tr')]
                //    .filter(x => [...x.querySelectorAll('td')][1].innerText == fechaFormateada)
                //    .map(x => {
                //        let [,,,time,,tsigno] = x.querySelectorAll('td');
                //        return {
                //            time: time.innerText,
                //            result: tsigno.innerText
                //        };
                //    });

                //    return r;
                //}", venezuelaNow.ToString("dd-MM-yyyy"));

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
                    let r = [...table.querySelectorAll('tbody tr')]
                    .filter(x => [...x.querySelectorAll('td')][1].innerText == fechaFormateada)
                    .map(x => {
                        let [,,,time,,,tsigno] = x.querySelectorAll('td');
                        return {
                            time: time.innerText,
                            result: tsigno.innerText
                        };
                    });

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(ZodiacoDelZuliaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.Time.ToUpper();
                    var premierId = TripleC[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(ZodiacoDelZuliaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ZodiacoDelZuliaOfficial));
                throw;
            }
        }
    }
}

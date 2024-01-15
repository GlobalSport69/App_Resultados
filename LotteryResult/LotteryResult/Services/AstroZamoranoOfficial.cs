using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class AstroZamoranoOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 22;
        private const int providerID = 22;
        private readonly ILogger<AstroZamoranoOfficial> _logger;

        public AstroZamoranoOfficial(IUnitOfWork unitOfWork, ILogger<AstroZamoranoOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                var venezuelaNow = DateTime.Now;

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
                _logger.LogInformation("///////////////////////////////////");
                _logger.LogInformation(DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss"));
                _logger.LogInformation(venezuelaNow.ToString("yyyy-MM-dd hh:mm:ss"));
                _logger.LogInformation(venezuelaNow.Date.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss"));
                _logger.LogInformation("///////////////////////////////////");

                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("http://triplezamorano.com/action/index");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    var fechaFormateada = date;

                    let iframe = document.querySelector('iframe')
                    let contenidoDelIframe = iframe.contentDocument || iframe.contentWindow.document;
                    let table = contenidoDelIframe.querySelector('#miTabla');
                    var result = [...table.querySelector('tbody').querySelectorAll('tr')].filter(x => ([...x.querySelectorAll('td')][1]).innerText == fechaFormateada)
                    let r = result.map(x => {
                        let [,,,time,, tsigno] = x.querySelectorAll('td');
                        return {
                            time: time.innerText,
                            result: tsigno.innerText
                        }
                    })

                    return r;
                }", venezuelaNow.ToString("dd-MM-yyyy"));

                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(AstroZamoranoOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in someObject)
                {
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.Result,
                        Time = item.Time,
                        Date = string.Empty,
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(AstroZamoranoOfficial));
                throw;
            }
        }
    }
}
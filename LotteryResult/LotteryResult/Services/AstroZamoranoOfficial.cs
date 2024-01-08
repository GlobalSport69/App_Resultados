using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
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
                await page.GoToAsync("http://triplezamorano.com/action/index");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    var fecha = new Date();
                    var dia = String(fecha.getDate()).padStart(2, '0');
                    var mes = String(fecha.getMonth() + 1).padStart(2, '0');
                    var ano = fecha.getFullYear();
                    var fechaFormateada = dia + '-' + mes + '-' + ano;

                    let iframe = document.querySelector('iframe')
                    let contenidoDelIframe = iframe.contentDocument || iframe.contentWindow.document;
                    let table = contenidoDelIframe.querySelector('#miTabla');
                    var result = [...table.querySelector('tbody').querySelectorAll('tr')].filter(x => ([...x.querySelectorAll('td')][1]).innerText == '07-01-2024')
                    let r = result.map(x => {
                        let [,,,time,, tsigno] = x.querySelectorAll('td');
                        return {
                            time: time.innerText,
                            result: tsigno.innerText
                        }
                    })

                    return r;
                }");

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
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
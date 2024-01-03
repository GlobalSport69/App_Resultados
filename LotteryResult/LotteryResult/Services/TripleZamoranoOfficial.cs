using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleZamoranoOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int zamoranoID = 2;
        private const int zamoranoProviderID = 1;
        private readonly ILogger<TripleZamoranoOfficial> _logger;

        public TripleZamoranoOfficial(IUnitOfWork unitOfWork, ILogger<TripleZamoranoOfficial> logger)
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
                    new LaunchOptions {
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
                    var result = [...table.querySelector('tbody').querySelectorAll('tr')].filter(x => ([...x.querySelectorAll('td')][1]).innerText == fechaFormateada)
                    let r = result.map(x => ({
                        result: ([...x.querySelectorAll('td')][4]).innerText,
                        time: ([...x.querySelectorAll('td')][3]).innerText
                    }))

                    return r;
                }");

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == zamoranoProviderID && 
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in someObject)
                {
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result { 
                        Result1 = item.Result,
                        Time = item.Time,
                        Date = string.Empty,
                        ProductId = zamoranoID,
                        ProviderId = zamoranoProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleZamoranoOfficial));
                throw;
            }
        }
    }
}

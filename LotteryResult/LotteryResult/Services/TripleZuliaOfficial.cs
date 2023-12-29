using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleZuliaOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        public const int tripleZuliaID = 6;
        private const int tripleZuliaProviderID = 5;
        private readonly ILogger<TripleZuliaOfficial> _logger;

        public TripleZuliaOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<TripleZuliaOfficial> logger)
        {
            this.resultRepository = resultRepository;
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
                await page.GoToAsync("http://www.resultadostriplezulia.com/action/index", waitUntil: WaitUntilNavigation.Networkidle2);

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let fecha = new Date();
                    let dia = String(fecha.getDate()).padStart(2, '0');
                    let mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Los meses en JavaScript empiezan desde 0
                    let ano = fecha.getFullYear();
                    let fechaFormateada = dia + '-' + mes + '-' + ano;

                    let iframe = document.querySelector('iframe')
                    let contenidoDelIframe = iframe.contentDocument || iframe.contentWindow.document;
                    let table = contenidoDelIframe.querySelector('#miTabla');

                    let r = [...table.querySelectorAll('tbody tr')]
                    .filter(x => [...x.querySelectorAll('td')][1].innerText == fechaFormateada)
                    .flatMap(x => {
                        let tds = [...x.querySelectorAll('td')];
                        let a ={
                            time: tds[3].innerText,
                            result: tds[4].innerText,
                            sorteo: 'Triple A'
                        };
                        let b ={
                            time: tds[3].innerText,
                            result: tds[5].innerText,
                            sorteo: 'Triple B'
                        };
                        return [a, b];
                    });

                    return r;
                }");

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == tripleZuliaProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in someObject)
                {
                    resultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.Result,
                        Time = item.Time,
                        Date = string.Empty,
                        ProductId = tripleZuliaID,
                        ProviderId = tripleZuliaProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = item.Sorteo
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleZuliaOfficial));
                throw;
            }
        }
    }
}

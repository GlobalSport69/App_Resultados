using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaRucaOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int laRucaID = 9;
        private const int laRucaProviderID = 8;
        private readonly ILogger<LaRucaOfficial> _logger;

        public LaRucaOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<LaRucaOfficial> logger)
        {
            this.resultRepository = resultRepository;
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
                //await page.GoToAsync("https://lottollano.com.ve/laruca.php");

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
                //        result: ([...tr.querySelectorAll('td')][1]).querySelector('img').getAttribute('src').replace('./laruca/', '').replace('.jpg', '')
                //    }))

                //    return r;
                //}");
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
                await page.GoToAsync("https://triples.bet/products-results/la-ruca-results");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.results-content-item')]
                    .map(x => ({
                        time: x.querySelector('.results-title-draw-hour').innerText,
                        result: x.querySelector('.number-ruca')?.innerText
                    })).filter(x => x.result !== undefined);

                    return r;
                }");

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == laRucaProviderID &&
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
                        Time = item.Time.ToUpper(),
                        Date = string.Empty,
                        ProductId = laRucaID,
                        ProviderId = laRucaProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TERMINALES
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LottoReyOfficial));
                throw;
            }
        }
    }
}

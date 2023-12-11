using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LottoReyOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;

        public LottoReyOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork)
        {
            this.resultRepository = resultRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Handel()
        {
            try
            {
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions { Headless = true });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://lottorey.com.ve");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let fecha = new Date();
                    let dia = String(fecha.getDate()).padStart(2, '0');
                    let mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Los meses en JavaScript empiezan desde 0
                    let ano = fecha.getFullYear();
                    let fechaFormateada = dia + '/' + mes + '/' + ano;

                    let r = [...document.querySelectorAll('#main-container .card')]
                    .filter(x => x.querySelector('.texto-fecha').innerText == fechaFormateada)
                    .map(x => ({
                        time: x.querySelector('.texto-hora').innerText,
                        result: x.querySelector('.card-footer').innerText,
                    }))

                    return r;
                }");

                const int lottoReyID = 5;
                const int lottoReyProviderID = 4;

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == lottoReyProviderID &&
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
                        ProductId = lottoReyID,
                        ProviderId = lottoReyProviderID,
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

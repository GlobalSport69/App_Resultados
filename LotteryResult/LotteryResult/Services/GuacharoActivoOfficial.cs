using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class GuacharoActivoOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int guacharoID = 11;
        private const int guacharoProviderID = 11;
        private readonly ILogger<GuacharoActivoOfficial> _logger;

        public GuacharoActivoOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<GuacharoActivoOfficial> logger)
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
                    new LaunchOptions { Headless = true });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://guacharoactivo.com/#/resultados-guacharo");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.col-md-3.mb-4.text-center')]
                    .map(x => ({
                        time: x.querySelector('.hour').innerText.replace('Resultado ', ''),
                        result: x.querySelector('img').getAttribute('src').replace('../../../animals/guacharo/', '').replace('.png', '') +' '+ x.querySelector('.name').innerText
                    }))
                    .filter(x => x.result != 'espera Esperando');

                    return r;
                }");

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == guacharoProviderID &&
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
                        ProductId = guacharoID,
                        ProviderId = guacharoProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TERMINALES
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(GuacharoActivoOfficial));
                throw;
            }
        }
    }
}

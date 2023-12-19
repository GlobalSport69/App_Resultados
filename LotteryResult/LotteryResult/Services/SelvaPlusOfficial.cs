using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class SelvaPlusOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int selvaPlusID = 10;
        private const int selvaPlusProviderID = 10;
        private readonly ILogger<SelvaPlusOfficial> _logger;

        public SelvaPlusOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<SelvaPlusOfficial> logger)
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
                await page.GoToAsync("https://guacharoactivo.com/#/resultados-selva-plus");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.col-md-3.mb-4.text-center')]
                    .map(x => ({
                        time: x.querySelector('.hour').innerText.replace('Resultado ', ''),
                        result: x.querySelector('img').getAttribute('src').replace('../../../animals/selva/', '').replace('.png', '') +' '+ x.querySelector('.name').innerText
                    }))
                    .filter(x => x.result != 'espera Esperando');

                    return r;
                }");

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == selvaPlusProviderID &&
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
                        ProductId = selvaPlusID,
                        ProviderId = selvaPlusProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALES77
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(SelvaPlusOfficial));
                throw;
            }
        }
    }
}

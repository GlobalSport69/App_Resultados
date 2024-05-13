using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleChanceOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 17;
        private const int providerID = 17;
        private readonly ILogger<TripleChanceOfficial> _logger;

        public TripleChanceOfficial(IUnitOfWork unitOfWork, ILogger<TripleChanceOfficial> logger)
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
                await page.GoToAsync("https://tuchance.com.ve/resultadosChance", waitUntil: WaitUntilNavigation.Networkidle2);
                //await page.WaitForNetworkIdleAsync();
                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.table tbody tr')]
                    .flatMap(x => {
                        let [t, a, b] = [...x.querySelectorAll('td span')]
                        return [
                            {
                                time: t.innerText,
                                result: a.innerText,
                                sorteo: 'Triple A'
                            },
                            {
                                time: t.innerText,
                                result: b.innerText,
                                sorteo: 'Triple B'
                            }
                        ]
                    })

                    return r;
                }");

                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleChanceOfficial));
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
                    var newTime = item.Time.Substring(0, item.Time.Length - 2) + " " + item.Time.Substring(item.Time.Length - 2);
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.Result,
                        Time = newTime,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = item.Sorteo
                    });
                }


                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleChanceOfficial));
                throw;
            }
        }
    }
}


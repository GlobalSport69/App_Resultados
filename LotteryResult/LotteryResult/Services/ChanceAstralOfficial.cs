using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class ChanceAstralOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 21;
        private const int providerID = 21;
        private readonly ILogger<ChanceAstralOfficial> _logger;

        public ChanceAstralOfficial(IUnitOfWork unitOfWork, ILogger<ChanceAstralOfficial> logger)
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
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://tuchance.com.ve/resultadosChance", waitUntil: WaitUntilNavigation.Networkidle2);

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('.table tbody tr')]
                        .map(x => {
                            let [t,,, c1, c2] = [...x.querySelectorAll('td span')]
                            return {
                              time: t.innerText,
                              result: c1.innerText+' '+c2.innerText
                            }
                        })

                    return r;
                }"
                );

                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(ChanceAstralOfficial));
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
                        Date = string.Empty,
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL,
                    });
                }


                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ChanceAstralOfficial));
                throw;
            }
        }
    }
}
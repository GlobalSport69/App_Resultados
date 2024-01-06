using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TachiraZodiacalOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 20;
        private const int providerID = 20;
        private readonly ILogger<TachiraZodiacalOfficial> _logger;

        public TachiraZodiacalOfficial(IUnitOfWork unitOfWork, ILogger<TachiraZodiacalOfficial> logger)
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
                await page.GoToAsync("https://tripletachira.com", waitUntil: WaitUntilNavigation.Networkidle2);

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                   let r = [...document.querySelectorAll('.single-ticket')]
                    .map(x => {
                        let title = x.querySelector('.ticket-rate').innerText;
                        if (title == '') return null;
                        let time = title.substring(title.length - 7, title.length - 2).trim() +' '+ title.substr(title.length - 2);
                        return ({
                            time: time, 
                            result: x.querySelector('.ticket-name')?.innerText,
                            sorteo:  title.substring(0, 9).trim()
                        });
                    }).filter(x => x != null && x.sorteo == 'ZODIACAL')

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
                        Time = LaGranjitaTerminalOfficial.FormatTime(item.Time),
                        Date = string.Empty,
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TachiraZodiacalOfficial));
                throw;
            }
        }
    }
}

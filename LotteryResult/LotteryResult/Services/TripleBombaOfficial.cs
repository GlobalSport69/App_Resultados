using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleBombaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int tripleBombaID = 15;
        private const int tripleBombaProviderID = 15;
        private readonly ILogger<TripleBombaOfficial> _logger;

        public TripleBombaOfficial(IUnitOfWork unitOfWork, ILogger<TripleBombaOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                // Obtén la zona horaria de Venezuela
                TimeZoneInfo venezuelaZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");

                // Obtén la fecha y hora actual en UTC
                DateTime utcNow = DateTime.UtcNow;

                // Convierte la fecha y hora actual a la zona horaria de Venezuela
                DateTime venezuelaNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, venezuelaZone);

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
                await page.GoToAsync("https://www.triplebomba.com/sistema", waitUntil: WaitUntilNavigation.Networkidle2);

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let dateText = document.querySelector('legend > h6').innerText;
                    if(dateText.substr(dateText.length - 10) != date) return [];

                    let r = [...document.querySelector('.card-deck').querySelectorAll('.card')]
                    .flatMap(x => [...x.querySelectorAll('.card-body .mx-auto div')]
                                .map(y => ({time: y.querySelector('h6').innerText.split('-')[1].trim(),
                                    result: y.querySelector('input').value,
                                    sorteo: y.querySelector('h6').innerText.split('-')[0].trim()
                                }))
                    ).filter(x => x.result !== '---')

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleBombaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == tripleBombaProviderID &&
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
                        Time = LaGranjitaTerminalOfficial.FormatTime(item.Time.ToUpper()),
                        Date = string.Empty,
                        ProductId = tripleBombaID,
                        ProviderId = tripleBombaProviderID,
                        Sorteo = item.Sorteo,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleBombaOfficial));
                throw;
            }
        }
    }
}

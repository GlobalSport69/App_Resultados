using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class SignoCalienteOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 23;
        private const int providerID = 23;
        private readonly ILogger<SignoCalienteOfficial> _logger;

        public SignoCalienteOfficial(IUnitOfWork unitOfWork, ILogger<SignoCalienteOfficial> logger)
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
                await page.GoToAsync("http://www.triplecaliente.com/action/index", waitUntil:WaitUntilNavigation.Networkidle2);

                //var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                //    //let fecha = new Date();
                //    //let dia = String(fecha.getDate()).padStart(2, '0');
                //    //let mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Los meses en JavaScript empiezan desde 0
                //    //let ano = fecha.getFullYear();
                //    //let fechaFormateada = dia + '-' + mes + '-' + ano;
                //    let fechaFormateada = date;

                //    let iframe = document.querySelector('iframe')
                //    let contenidoDelIframe = iframe.contentDocument || iframe.contentWindow.document;
                //    let table = contenidoDelIframe.querySelector('#miTabla');

                //    let r = [...table.querySelectorAll('tbody tr')]
                //    .filter(x => [...x.querySelectorAll('td')][1].innerText == fechaFormateada)
                //    .map(x => {
                //        let [,,,time,,,tsigno] = x.querySelectorAll('td');
                //        return {
                //            time: time.innerText,
                //            result: tsigno.innerText.replace(' - ', ' ')
                //        };
                //    });

                //    return r;
                //}", venezuelaNow.ToString("dd-MM-yyyy"));
                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('table tr td');
                    return tds.length > 1;
                }", new WaitForFunctionOptions
                {
                    PollingInterval = 1000,
                });

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let fechaFormateada = date;
                    let table = document.querySelector('table');
                    let r = [...table.querySelectorAll('tbody tr')]
                    .filter(x => [...x.querySelectorAll('td')][1].innerText == fechaFormateada)
                    .map(x => {
                        let [,,,time,,,tsigno] = x.querySelectorAll('td');
                        return {
                            time: time.innerText,
                            result: tsigno.innerText.replace(' - ', ' ')
                        };
                    });

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(SignoCalienteOfficial));
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
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.Result,
                        Time = item.Time,
                        Date = string.Empty,
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL,
                        Sorteo = item.Sorteo
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(SignoCalienteOfficial));
                throw;
            }
        }
    }
}

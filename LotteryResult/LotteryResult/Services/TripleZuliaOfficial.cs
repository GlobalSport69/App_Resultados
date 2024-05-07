using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleZuliaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int tripleZuliaID = 6;
        private const int tripleZuliaProviderID = 5;
        private readonly ILogger<TripleZuliaOfficial> _logger;

        public TripleZuliaOfficial(IUnitOfWork unitOfWork, ILogger<TripleZuliaOfficial> logger)
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
                await page.GoToAsync("http://www.resultadostriplezulia.com/action/index", waitUntil: WaitUntilNavigation.Networkidle2);

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
                //    .flatMap(x => {
                //        let tds = [...x.querySelectorAll('td')];
                //        let a ={
                //            time: tds[3].innerText,
                //            result: tds[4].innerText,
                //            sorteo: 'Triple A'
                //        };
                //        let b ={
                //            time: tds[3].innerText,
                //            result: tds[5].innerText,
                //            sorteo: 'Triple B'
                //        };
                //        return [a, b];
                //    });

                //    return r;
                //}", venezuelaNow.ToString("dd-MM-yyyy"));
                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let fechaFormateada = date;

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
                }", venezuelaNow.ToString("dd-MM-yyyy"));


                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleZuliaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == tripleZuliaProviderID && x.CreatedAt.Date == venezuelaNow.Date);
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

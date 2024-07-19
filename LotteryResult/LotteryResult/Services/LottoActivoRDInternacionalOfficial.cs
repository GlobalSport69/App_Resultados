using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LottoActivoRDInternacionalOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 29;
        private const int providerID = 28;
        private readonly ILogger<LottoActivoRDInternacionalOfficial> _logger;

        public LottoActivoRDInternacionalOfficial(IUnitOfWork unitOfWork, ILogger<LottoActivoRDInternacionalOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            //var a = new NotifyPremierService();
            //a.Handler(2);
            //return;
            try
            {
                DateTime venezuelaNow = DateTime.Now;
                var date = DateTime.Now.ToString("yyyy-MM-dd");

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
                await page.GoToAsync($"https://www.lottoactivo.com/resultados/lotto_activo_internacional/{date}/", waitUntil: WaitUntilNavigation.Networkidle2);

                // Espera hasta que haya al menos 1 elementos 'div' dentro de '#resultados'
                await page.WaitForFunctionAsync(@"() => {
                    const tds = document.querySelectorAll('#resultados div');
                    return tds.length > 0;
                }", new WaitForFunctionOptions
                {
                    PollingInterval = 1000,
                });

                var response = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    let r = [...document.querySelectorAll('#resultados div')]
                        .map(row =>{ 
                          const spanElement = row.querySelector('span'); 
                          if(spanElement === null){
                            return spanElement;
                          }
                          return { 
                            result: spanElement.textContent,
                            time: row.querySelector('p').innerText.replace('LOTTO ACTIVO RD INTERNACIONAL', '').trim(),
                            complement: spanElement.nextSibling.textContent.trim()
                          }
                        }).filter(x => x !== null)

                    return r;
                }");


                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoActivoRDInternacionalOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {

                    item.Complement = item.Complement.Capitalize();
                    var resultado = item.Result + " " +item.Complement;
                    var time = item.Time.ToUpper();

                    return new Result
                    {
                        Result1 = resultado,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        //Number: item.Result,
                        //Complement: complement
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();

                var toUpdate = new List<Result>();
                foreach (var item in newResult)
                {
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Sorteo == y.Sorteo && item.Result1 != y.Result1);
                    if (found is null)
                        continue;

                    found.Result1 = item.Result1;
                    toUpdate.Add(found);
                }
                var toInsert = newResult.Where(x => !oldResult.Exists(y => x.Time == y.Time && x.Sorteo == y.Sorteo));
                var needSave = false;
                foreach (var item in toUpdate)
                {
                    unitOfWork.ResultRepository.Update(item);
                    needSave = true;
                }
                foreach (var item in toInsert)
                {
                    unitOfWork.ResultRepository.Insert(item);
                    needSave = true;
                }

                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LottoActivoRDInternacionalOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LottoActivoRDInternacionalOfficial));
                throw;
            }
        }
    }
}

using LotteryResult.Dtos;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LoteriaDeHoy : IGetResult
    {
        public async Task Handel()
        {
            try
            {
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions { Headless = true });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://loteriadehoy.com/animalitos/resultados/");

                var someObject = await page.EvaluateFunctionAsync<GetResultResponseDto>(@"() => 
                [...document.querySelectorAll('section .js-con')]
                .map((x, index) => 
                ({ 
                    lottery: ([...document.querySelectorAll('section .title-center h1')][index]).innerText,
                    results: [...x.querySelectorAll('.circle-legend')]
                    .map(y => 
                    ({ 
                        result: y.querySelector('h4').innerText, 
                        time: y.querySelector('h5').innerText 
                    })) 
                }))");

                Console.WriteLine(someObject);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

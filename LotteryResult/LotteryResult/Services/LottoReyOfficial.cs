﻿using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LottoReyOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int lottoReyID = 5;
        private const int lottoReyProviderID = 4;
        private readonly ILogger<LottoReyOfficial> _logger;

        public LottoReyOfficial(IUnitOfWork unitOfWork, ILogger<LottoReyOfficial> logger)
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
                await page.GoToAsync("https://lottorey.com.ve");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"(date) => {
                    let fechaFormateada = date;

                    let r = [...document.querySelectorAll('#main-container .card')]
                    .filter(x => x.querySelector('.texto-fecha').innerText == fechaFormateada)
                    .map(x => ({
                        time: x.querySelector('.texto-hora').innerText,
                        result: x.querySelector('.card-footer').innerText,
                    }))

                    return r;
                }", venezuelaNow.ToString("dd/MM/yyyy"));

                if (!someObject.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoReyOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == lottoReyProviderID && x.CreatedAt.Date == venezuelaNow.Date);

                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in someObject)
                {
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.Result,
                        Time = item.Time.ToUpper(),
                        Date = string.Empty,
                        ProductId = lottoReyID,
                        ProviderId = lottoReyProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception:ex, message: nameof(LottoReyOfficial));
                throw;
            }
        }
    }
}

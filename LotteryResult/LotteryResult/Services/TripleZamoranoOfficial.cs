﻿using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleZamoranoOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;

        public TripleZamoranoOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork)
        {
            this.resultRepository = resultRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Handel()
        {
            try
            {
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions { Headless = true });
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("http://triplezamorano.com/action/index");

                var someObject = await page.EvaluateFunctionAsync<List<LotteryDetail>>(@"() => {
                    var fecha = new Date();
                    var dia = String(fecha.getDate()).padStart(2, '0');
                    var mes = String(fecha.getMonth() + 1).padStart(2, '0');
                    var ano = fecha.getFullYear();
                    var fechaFormateada = dia + '-' + mes + '-' + ano;

                    let iframe = document.querySelector('iframe')
                    let contenidoDelIframe = iframe.contentDocument || iframe.contentWindow.document;
                    let table = contenidoDelIframe.querySelector('#miTabla');
                    var result = [...table.querySelector('tbody').querySelectorAll('tr')].filter(x => ([...x.querySelectorAll('td')][1]).innerText == fechaFormateada)
                    let r = result.map(x => ({
                        result: ([...x.querySelectorAll('td')][4]).innerText,
                        time: ([...x.querySelectorAll('td')][3]).innerText
                    }))

                    return r;
                }");

                const int zamoranoID = 2;
                const int zamoranoProviderID = 1;

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == zamoranoProviderID && 
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in someObject)
                {
                    resultRepository.Insert(new Data.Models.Result { 
                        Result1 = item.Result,
                        Time = item.Time,
                        Date = string.Empty,
                        ProductId = zamoranoID,
                        ProviderId = zamoranoProviderID,
                    });
                }

                Console.WriteLine(someObject);

                await unitOfWork.SaveChangeAsync();
                //return new GetResultResponseDto
                //{
                //    LotteryName = "TRIPLE ZAMORANO",
                //    Results = someObject
                //};
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

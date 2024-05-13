using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaRicachonaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int laRicachonaID = 12;
        private const int laRicachonaProviderID = 12;
        private readonly ILogger<LaRicachonaOfficial> _logger;

        public LaRicachonaOfficial(IUnitOfWork unitOfWork, ILogger<LaRicachonaOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var response = await "https://webservice.premierpluss.com"
                .AppendPathSegments("loteries", "results3")
                .SetQueryParams(new
                {
                    since = venezuelaNow.ToString("yyyy-MM-dd"),
                    product = 9
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaRicachonaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == laRicachonaProviderID && x.CreatedAt.Date == venezuelaNow.Date);
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response)
                {
                    var time = item.lottery.name.Replace("LA RICACHONA ", "").Replace("O", "0").ToUpper();
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.result,
                        Time = LaGranjitaTerminalOfficial.FormatTime(time),
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = laRicachonaID,
                        ProviderId = laRicachonaProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES
                    });
                }

                //Console.WriteLine(response);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaRicachonaOfficial));
                throw;
            }
        }
    }
}

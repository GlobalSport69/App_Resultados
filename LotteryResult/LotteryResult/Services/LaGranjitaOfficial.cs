using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaGranjitaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int laGranjitaID = 1;
        private const int laGranjitaProviderID = 3;
        private readonly ILogger<LaGranjitaOfficial> _logger;

        public LaGranjitaOfficial(IUnitOfWork unitOfWork, ILogger<LaGranjitaOfficial> logger)
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
                    product = 1
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaGranjitaOfficial));
                    return; 
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == laGranjitaProviderID && x.CreatedAt.Date == venezuelaNow.Date);

                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response)
                {
                    var time = item.lottery.name.Replace("LA GRANJITA ", "").Replace("O", "0").ToUpper();

                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.result.Replace("-", " "),
                        Time = LaGranjitaTerminalOfficial.FormatTime(time),
                        Date = string.Empty,
                        ProductId = laGranjitaID,
                        ProviderId = laGranjitaProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaGranjitaOfficial));
                throw;
            }
        }
    }
}

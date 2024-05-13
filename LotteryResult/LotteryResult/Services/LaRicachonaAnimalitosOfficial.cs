using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services
{
    public class LaRicachonaAnimalitosOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int laRicachonaAnimalitosID = 14;
        private const int laRicachonaAnimalitosProviderID = 14;
        private readonly ILogger<LaRicachonaAnimalitosOfficial> _logger;

        public LaRicachonaAnimalitosOfficial(IUnitOfWork unitOfWork, ILogger<LaRicachonaAnimalitosOfficial> logger)
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
                    product = 16
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaRicachonaAnimalitosOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == laRicachonaAnimalitosProviderID && x.CreatedAt.Date == venezuelaNow.Date);

                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response)
                {
                    var time = item.lottery.name.Replace("ANIMALITOS LA RICACHONA ", "").Replace("O", "0").ToUpper();
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.result.Replace("-", " "),
                        Time = LaGranjitaTerminalOfficial.FormatTime(time),
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = laRicachonaAnimalitosID,
                        ProviderId = laRicachonaAnimalitosProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaRicachonaAnimalitosOfficial));
                throw;
            }
        }
    }
}

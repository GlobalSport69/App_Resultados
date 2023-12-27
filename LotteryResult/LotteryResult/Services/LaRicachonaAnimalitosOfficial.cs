using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services
{
    public class LaRicachonaAnimalitosOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int laGranjitaID = 14;
        private const int laGranjitaProviderID = 14;
        private readonly ILogger<LaRicachonaAnimalitosOfficial> _logger;

        public LaRicachonaAnimalitosOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<LaRicachonaAnimalitosOfficial> logger)
        {
            this.resultRepository = resultRepository;
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

                var response = await "https://webservice.premierpluss.com"
                .AppendPathSegments("loteries", "results3")
                .SetQueryParams(new
                {
                    since = venezuelaNow.ToString("yyyy-MM-dd"),
                    product = 16
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == laGranjitaProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);

                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in response)
                {
                    var time = item.lottery.name.Replace("ANIMALITOS LA RICACHONA ", "").Replace("O", "0").ToUpper();
                    resultRepository.Insert(new Data.Models.Result
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
                _logger.LogError(exception: ex, message: nameof(LaRicachonaAnimalitosOfficial));
                throw;
            }
        }
    }
}

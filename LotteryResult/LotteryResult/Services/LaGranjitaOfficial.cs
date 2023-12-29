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
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        public const int laGranjitaID = 1;
        private const int laGranjitaProviderID = 3;
        private readonly ILogger<LaGranjitaOfficial> _logger;

        public LaGranjitaOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<LaGranjitaOfficial> logger)
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
                    product = 1
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
                    var time = item.lottery.name.Replace("LA GRANJITA ", "").Replace("O", "0").ToUpper();

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
                _logger.LogError(exception: ex, message: nameof(LaGranjitaOfficial));
                throw;
            }
        }
    }
}

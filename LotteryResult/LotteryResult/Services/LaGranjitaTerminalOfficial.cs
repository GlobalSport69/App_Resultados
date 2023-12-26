using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services
{
    public class LaGranjitaTerminalOfficial
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int laGranjitaTerminalesID = 14;
        private const int laGranjitaTerminalesProviderID = 14;
        private readonly ILogger<LaGranjitaTerminalOfficial> _logger;

        public LaGranjitaTerminalOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<LaGranjitaTerminalOfficial> logger)
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
                    product = 25
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == laGranjitaTerminalesProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);

                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in response)
                {
                    resultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.result,
                        Time = item.lottery.name.Replace("TERMINAL LA GRANJITA ", "").Replace("O", "0").ToUpper(),
                        Date = string.Empty,
                        ProductId = laGranjitaTerminalesID,
                        ProviderId = laGranjitaTerminalesProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TERMINALES
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaGranjitaTerminalOfficial));
                throw;
            }
        }
    }
}

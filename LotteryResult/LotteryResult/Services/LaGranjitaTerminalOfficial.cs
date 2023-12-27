using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using System.Globalization;

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
                    var stringTime = item.lottery.name.Replace("TERMINAL LA GRANJITA ", "").Replace("O", "0").ToUpper();
                    resultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.result,
                        Time = FormatTime(stringTime),
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

        public static string FormatTime(string time) {
            DateTime dateTime;

            var cultureInfo = new CultureInfo("en-US");
            if (DateTime.TryParseExact(time, "h:mm tt", cultureInfo, DateTimeStyles.None, out dateTime))
            {
                string formattedTime = dateTime.ToString("hh:mm tt", cultureInfo);
                return formattedTime.ToUpper();
            }

            throw new Exception("El formato de la hora proporcionada no es válido.");
        }
    }
}

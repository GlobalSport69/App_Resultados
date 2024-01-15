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
        private IUnitOfWork unitOfWork;
        public const int laGranjitaTerminalesID = 13;
        private const int laGranjitaTerminalesProviderID = 13;
        private readonly ILogger<LaGranjitaTerminalOfficial> _logger;

        public LaGranjitaTerminalOfficial(IUnitOfWork unitOfWork, ILogger<LaGranjitaTerminalOfficial> logger)
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
                    product = 25
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaGranjitaTerminalOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == laGranjitaTerminalesProviderID && x.CreatedAt.Date == venezuelaNow.Date);

                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response)
                {
                    var stringTime = item.lottery.name.Replace("TERMINAL LA GRANJITA ", "").Replace("O", "0").ToUpper();
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
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

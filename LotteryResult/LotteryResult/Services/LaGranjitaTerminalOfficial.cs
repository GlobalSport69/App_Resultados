using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using System.Globalization;

namespace LotteryResult.Services
{
    public class LaGranjitaTerminalOfficial
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 13;
        private const int providerID = 13;
        private readonly ILogger<LaGranjitaTerminalOfficial> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "08:05 AM", 240 },
            { "09:05 AM", 241 },
            { "10:05 AM", 242 },
            { "11:05 AM", 243 },
            { "12:05 PM", 244 },
            { "01:05 PM", 245 },
            { "02:05 PM", 246 },
            { "03:05 PM", 247 },
            { "04:05 PM", 248 },
            { "05:05 PM", 249 },
            { "06:05 PM", 250 },
            { "07:05 PM", 251 }
        };
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
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.lottery.name.Replace("TERMINAL LA GRANJITA ", "").Replace("O", "0").ToUpper();
                    var PremierId = lotteries[LaGranjitaTerminalOfficial.FormatTime(time)];

                    return new Result
                    {
                        Result1 = item.result,
                        Time = FormatTime(time),
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TERMINALES,
                        PremierId = PremierId
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();

                var needSave = false;
                // no hay resultado nuevo
                var len = oldResult.Count();
                if (len == newResult.Count())
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (oldResult[i].Time == newResult[i].Time && oldResult[i].Result1 != newResult[i].Result1)
                        {
                            oldResult[i].Result1 = newResult[i].Result1;
                            unitOfWork.ResultRepository.Update(oldResult[i]);
                            needSave = true;
                        }
                    }
                }

                // hay resultado nuevo
                if (newResult.Count() > len)
                {
                    var founds = newResult.Where(x => !oldResult.Any(y => y.Time == x.Time));

                    foreach (var item in founds)
                    {
                        unitOfWork.ResultRepository.Insert(item);
                        needSave = true;
                    }
                }

                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaGranjitaTerminalOfficial));
                    return;
                }
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

        public static TimeOnly ConvertToTime(string time)
        {
            TimeOnly dateTime;

            var cultureInfo = new CultureInfo("en-US");
            if (TimeOnly.TryParseExact(time, "h:mm tt", cultureInfo, DateTimeStyles.None, out dateTime))
            {
                return dateTime;
            }

            throw new Exception("El formato de la hora proporcionada no es válido.");
        }
    }
}

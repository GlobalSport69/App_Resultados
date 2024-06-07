using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using System.Globalization;

namespace LotteryResult.Services
{
    public class ZodiacalCaracasOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 18;
        private const int providerID = 18;
        private readonly ILogger<ZodiacalCaracasOfficial> _logger;
        //private Dictionary<string, long> lotteries = new Dictionary<string, long>
        //{
        //    { "01:00 PM", 148 },
        //    { "04:30 PM", 166 },
        //    { "07:00 PM", 151 }
        //};
        public ZodiacalCaracasOfficial(IUnitOfWork unitOfWork, ILogger<ZodiacalCaracasOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var results = await "http://165.227.185.69:8000"
                .AppendPathSegments("api", "resultados")
                .SetQueryParams(new
                {
                    fecha = venezuelaNow.ToString("yyyy-MM-dd"),
                })
                .GetJsonAsync<TripleCaracasOfficialResponse>();

                var response = results.resultados
                    .GroupBy(x => x.producto_juego.order)
                    .OrderBy(x => x.Key)
                    .Where(x => x.Key == 3)
                    .SelectMany(x => x)
                    .ToList();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(ZodiacalCaracasOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var sorteos = await unitOfWork.LotteryRepository
                    .GetAllByAsync(x => x.ProductId == productID);
                var lotteries = sorteos.ToDictionary(x => x.LotteryHour.FormatTime());

                var newResult = response.Select(item => {

                    //Crear un DateTime a partir de la cadena de texto "13:00:00"
                    DateTime dt = DateTime.ParseExact(item.sorteo.hora, "HH:mm:ss", CultureInfo.InvariantCulture);
                    // Convertir a formato de 12 horas (AM/PM)
                    string time = dt.ToString("hh:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    var sorteo = lotteries[time];

                    return new Result
                    {
                        Result1 = item.resultado + " " + item.resultado_elemento,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL,

                        LotteryId = sorteo.Id,
                        PremierId = sorteo.PremierId,
                        Number = item.resultado,
                        Animal = item.resultado_elemento,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(ZodiacalCaracasOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ZodiacalCaracasOfficial));
                throw;
            }
        }
    }
}
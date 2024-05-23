using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class SelvaPlusOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 10;
        private const int providerID = 10;
        private readonly ILogger<SelvaPlusOfficial> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "08:00 AM", 288 },
            { "09:00 AM", 207 },
            { "10:00 AM", 208 },
            { "11:00 AM", 209 },
            { "12:00 PM", 210 },
            { "01:00 PM", 211 },
            { "02:00 PM", 212 },
            { "03:00 PM", 213 },
            { "04:00 PM", 214 },
            { "05:00 PM", 215 },
            { "06:00 PM", 216 },
            { "07:00 PM", 217 }
        };
        public SelvaPlusOfficial(IUnitOfWork unitOfWork, ILogger<SelvaPlusOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var response = await "https://www.apuestanext.com/apuestanext.com/aplicativo/accion/apis/resultados.php"
                .PostJsonAsync(new
                {
                    id_tipo_loteria = 94,
                    fecha = venezuelaNow.ToString("yyyy-MM-dd")
                })
                .ReceiveJson<List<GuacharoOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(SelvaPlusOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository.GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.loteria.Replace("Selva Plus ", "").ToUpper();
                    var PremierId = lotteries[time];

                    return new Result
                    {
                        Result1 = item.numero + " " + item.nombre,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(SelvaPlusOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(SelvaPlusOfficial));
                throw;
            }
        }
    }
}

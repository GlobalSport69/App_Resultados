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
        private INotifyPremierService notifyPremierService;

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
        public SelvaPlusOfficial(IUnitOfWork unitOfWork, ILogger<SelvaPlusOfficial> logger, INotifyPremierService notifyPremierService)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
            this.notifyPremierService = notifyPremierService;
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
                    item.nombre = item.nombre.Capitalize();

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

                var toUpdate = new List<Result>();
                foreach (var item in newResult)
                {
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Result1 != y.Result1 && item.Sorteo == y.Sorteo);
                    if (found is null)
                        continue;

                    found.Result1 = item.Result1;
                    toUpdate.Add(found);
                }
                var toInsert = newResult.Where(x => !oldResult.Exists(y => x.Time == y.Time && x.Sorteo == y.Sorteo));
                var needSave = false;
                foreach (var item in toUpdate)
                {
                    unitOfWork.ResultRepository.Update(item);
                    needSave = true;
                }
                foreach (var item in toInsert)
                {
                    unitOfWork.ResultRepository.Insert(item);
                    needSave = true;
                }


                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();

                    if (toUpdate.Any())
                        notifyPremierService.Handler(toUpdate.Select(x => x.Id).ToList(), NotifyType.Update);
                    if (toInsert.Any())
                        notifyPremierService.Handler(toInsert.Select(x => x.Id).ToList(), NotifyType.New);
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

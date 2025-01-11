using Azure;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services
{
    public class LottoReyOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 5;
        private const int providerID = 4;
        private readonly ILogger<LottoReyOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "08:30 AM", 289 },
            { "09:30 AM", 67 },
            { "10:30 AM", 68 },
            { "11:30 AM", 69 },
            { "12:30 PM", 70 },
            { "01:30 PM", 71 },
            { "02:30 PM", 72 },
            { "03:30 PM", 73 },
            { "04:30 PM", 74 },
            { "05:30 PM", 75 },
            { "06:30 PM", 76 },
            { "07:30 PM", 77 }
        };

        private Dictionary<string, string> Hours = new Dictionary<string, string>
        {
            { "08:30:00", "08:30 AM"},
            { "09:30:00", "09:30 AM" },
            { "10:30:00", "10:30 AM" },
            { "11:30:00", "11:30 AM" },
            { "12:30:00", "12:30 PM" },
            { "13:30:00", "01:30 PM" },
            { "14:30:00", "02:30 PM" },
            { "15:30:00", "03:30 PM" },
            { "16:30:00", "04:30 PM" },
            { "17:30:00", "05:30 PM" },
            { "18:30:00", "06:30 PM" },
            { "19:30:00", "07:30 PM" }
        };

        private Dictionary<string, string> Animals = new Dictionary<string, string>
        {
            { "0", "DELFIN"},
            { "00", "BALLENA" },
            { "01", "CARNERO" },
            { "02", "TORO" },
            { "03", "CIEMPIES" },
            { "04", "ALACRAN" },
            { "05", "LEON" },
            { "06", "RANA" },
            { "07", "PERICO" },
            { "08", "RATON" },
            { "09", "AGUILA" },
            { "10", "TIGRE" },
            { "11", "GATO" },
            { "12", "CABALLO" },
            { "13", "MONO" },
            { "14", "PALOMA" },
            { "15", "ZORRO" },
            { "16", "OSO" },
            { "17", "PAVO" },
            { "18", "BURRO" },
            { "19", "CHIVO" },
            { "20", "COCHINO" },
            { "21", "GALLO" },
            { "22", "CAMELLO" },
            { "23", "CEBRA" },
            { "24", "IGUANA" },
            { "25", "GALLINA" },
            { "26", "VACA" },
            { "27", "PERRO" },
            { "28", "ZAMURO" },
            { "29", "ELEFANTE" },
            { "30", "CAIMAN" },
            { "31", "LAPA" },
            { "32", "ARDILLA" },
            { "33", "PESCADO" },
            { "34", "VENADO" },
            { "35", "JIRAFA" },
            { "36", "CULEBRA" },
        };
        public LottoReyOfficial(IUnitOfWork unitOfWork, ILogger<LottoReyOfficial> logger, INotifyPremierService notifyPremierService)
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

                var response = await "https://api.sourcesws.com/api/resultados"
                    .WithOAuthBearerToken("98844|MhLHygrtJY02GLOvNlb5KXafxtMK6tGsz5G3KSR3")
                    .GetJsonAsync<LottoReyResponse>();

                if (!response.success) {
                    _logger.LogInformation("Ocurrio un error en {0} {@1}", nameof(LottoReyOfficial), response);
                    return;
                }
                
                response.data.resultado_list = response.data.resultado_list.Where(x => x.fecha == venezuelaNow.ToString("yyyy-MM-dd")).ToList();

                if (!response.data.resultado_list.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoReyOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.data.resultado_list.Select(item => {
                    var time = Hours[item.sorteo];
                    var premierId = lotteries[time];

                    var number = item.resultado;
                    var complement = Animals[number]
                    .Trim()
                    .Capitalize();

                    return new Result
                    {
                        Result1 = $"{number} {complement}",
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = premierId,
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();


                var toUpdate = new List<Result>();
                foreach (var item in newResult)
                {
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Sorteo == y.Sorteo && item.Result1 != y.Result1);
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LottoReyOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception:ex, message: nameof(LottoReyOfficial));
                throw;
            }
        }
    }
}

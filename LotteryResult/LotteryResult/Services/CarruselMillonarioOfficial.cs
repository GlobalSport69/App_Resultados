using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Models.CarruselMillonario;
using Newtonsoft.Json.Linq;

namespace LotteryResult.Services
{
    public class CarruselMillonario : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 31;
        private const int providerID = 30;
        private readonly ILogger<CarruselMillonario> _logger;
        private readonly INotifyPremierService notifyPremierService;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "08:00 AM", 239 },
            { "09:00 AM", 2 },
            { "10:00 AM", 3 },
            { "11:00 AM", 4},
            { "12:00 PM", 5 },
            { "01:00 PM", 6 },
            { "02:00 PM", 7 },
            { "03:00 PM", 8 },
            { "04:00 PM", 9 },
            { "05:00 PM", 10 },
            { "06:00 PM", 11 },
            { "07:00 PM", 12 }
        };

        private Dictionary<string, string> hoursMap = new Dictionary<string, string>
        {
            { "8:00 AM", "08:00 AM" },
            { "9:00 AM", "09:00 AM" },
            { "10:00 AM", "10:00 AM" },
            { "11:00 AM", "11:00 AM"},
            { "12:00 M", "12:00 PM" },
            { "1:00 PM", "01:00 PM" },
            { "2:00 PM", "02:00 PM" },
            { "3:00 PM", "03:00 PM" },
            { "4:00 PM", "04:00 PM" },
            { "5:00 PM", "05:00 PM" },
            { "6:00 PM", "06:00 PM" },
            { "7:00 PM", "07:00 PM" }
        };

        public CarruselMillonario(IUnitOfWork unitOfWork, ILogger<CarruselMillonario> logger, INotifyPremierService notifyPremierService)
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

                var loginResponse = await "https://carruselmillonario.com/juegos/api/getToken.php"
                    .PostJsonAsync(new
                    {
                        identidad = "plustaquilla",
                        credencial = "123123",
                    })
                    .ReceiveJson<JObject>();
                if (!loginResponse.ContainsKey("token") || string.IsNullOrEmpty(loginResponse.Value<string>("token")))
                {
                    throw new Exception("Falta token "+ loginResponse);
                }

                var resultResponse = await "https://carruselmillonario.com/juegos/api/result.php"
                    .WithHeader("token", loginResponse.Value<string>("token"))
                    .PostJsonAsync(new
                    {
                        grupo_id = 2,
                        fecha = venezuelaNow.ToString("yyyy-MM-dd")
                    })
                    .ReceiveJson<CarruselMResultsResponseDto>();

                if (!resultResponse.result.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaGranjitaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = resultResponse.result.SelectMany(item =>
                {
                    var time = item.sorteo.Replace("Carrusel M", "").Trim().ToUpper();

                    var numbers = item.numero.Split('-');
                    var complements = item.resultado.Split('-');
                    var sorteosLabel = new string[] { "A", "B", "C" };
                    //var premierId = lotteries[hoursMap[time]];

                    return numbers.Select((x, i) =>
                    {
                        var animal = complements[i].Capitalize();
                        var number = x;
                        var resultado = i < 2 ? (number + " " + animal) : complements[i];

                        return new Result
                        {
                            Result1 = resultado,
                            Time = hoursMap[time],
                            Date = DateTime.Now.ToString("dd-MM-yyyy"),
                            Sorteo = sorteosLabel[i],
                            ProductId = productID,
                            ProviderId = providerID,
                            ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,

                            //PremierId = premierId,
                            //number = number,
                            //animal = animal
                        };
                    });
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(CarruselMillonario));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(CarruselMillonario));
                throw;
            }
        }
    }
}

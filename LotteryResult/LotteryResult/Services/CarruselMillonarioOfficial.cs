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

        private Dictionary<string, long> lotteriesA = new Dictionary<string, long>
        {
            { "09:00 AM", 323 },
            { "10:00 AM", 324 },
            { "11:00 AM", 325 },
            { "12:00 PM", 326 },
            { "01:00 PM", 327 },
            { "02:00 PM", 328 },
            { "04:00 PM", 329 },
            { "05:00 PM", 330 },
            { "06:00 PM", 331 },
            { "07:00 PM", 332 }
        };

        private Dictionary<string, string> hoursMap = new Dictionary<string, string>
        {
            { "09:00 AM", "09:00 AM" },
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

        private Dictionary<string, Tuple<string, string>> animals = new Dictionary<string, Tuple<string, string>>
        {
            ["47"] = new Tuple<string, string>("0", "DELFIN"),
            ["00"] = new Tuple<string, string>("00", "BALLENA"),
            ["01"] = new Tuple<string, string>("01", "CARNERO"),
            ["02"] = new Tuple<string, string>("02", "TORO"),
            ["03"] = new Tuple<string, string>("03", "CIEMPIÉS"),
            ["04"] = new Tuple<string, string>("04", "ALACRÁN"),
            ["05"] = new Tuple<string, string>("05", "LEÓN"),
            ["06"] = new Tuple<string, string>("06", "RANA"),
            ["07"] = new Tuple<string, string>("07", "PERICO"),
            ["08"] = new Tuple<string, string>("08", "RATÓN"),
            ["09"] = new Tuple<string, string>("09", "ÁGUILA"),
            ["10"] = new Tuple<string, string>("10", "TIGRE"),
            ["11"] = new Tuple<string, string>("11", "GATO"),
            ["12"] = new Tuple<string, string>("12", "CABALLO"),
            ["13"] = new Tuple<string, string>("13", "MONO"),
            ["14"] = new Tuple<string, string>("14", "PALOMA"),
            ["15"] = new Tuple<string, string>("15", "ZORRO"),
            ["16"] = new Tuple<string, string>("16", "OSO"),
            ["17"] = new Tuple<string, string>("17", "PAVO"),
            ["18"] = new Tuple<string, string>("18", "BURRO"),
            ["19"] = new Tuple<string, string>("19", "CHIVO"),
            ["20"] = new Tuple<string, string>("20", "COCHINO"),
            ["21"] = new Tuple<string, string>("21", "GALLO"),
            ["22"] = new Tuple<string, string>("22", "CAMELLO"),
            ["23"] = new Tuple<string, string>("23", "CEBRA"),
            ["24"] = new Tuple<string, string>("24", "IGUANA"),
            ["25"] = new Tuple<string, string>("25", "GALLINA"),
            ["26"] = new Tuple<string, string>("26", "VACA"),
            ["27"] = new Tuple<string, string>("27", "PERRO"),
            ["28"] = new Tuple<string, string>("28", "ZAMURO"),
            ["29"] = new Tuple<string, string>("29", "ELEFANTE"),
            ["30"] = new Tuple<string, string>("30", "CAIMÁN"),
            ["31"] = new Tuple<string, string>("31", "LAPA"),
            ["32"] = new Tuple<string, string>("32", "ARDILLA"),
            ["33"] = new Tuple<string, string>("33", "PESCADO"),
            ["34"] = new Tuple<string, string>("34", "VENADO"),
            ["35"] = new Tuple<string, string>("35", "JIRAFA"),
            ["36"] = new Tuple<string, string>("36", "CULEBRA"),
            ["37"] = new Tuple<string, string>("37", "GARZA"),
            ["38"] = new Tuple<string, string>("38", "CUCARACHA"),
            ["39"] = new Tuple<string, string>("39", "GANSO"),
            ["40"] = new Tuple<string, string>("40", "PELICANO"),
            ["41"] = new Tuple<string, string>("41", "CHIGÜIRE"),
            ["42"] = new Tuple<string, string>("42", "AVESTRUZ"),
            ["43"] = new Tuple<string, string>("43", "HIPOPÓTAMO"),
            ["44"] = new Tuple<string, string>("44", "MARIPOSA"),
            ["45"] = new Tuple<string, string>("45", "FLAMINGO"),
            ["46"] = new Tuple<string, string>("46", "PATO")
        };
        private Dictionary<string, string> sing = new Dictionary<string, string>
        {
            ["ARI"] = "ARIES",
            ["TAU"] = "TAURO",
            ["GEM"] = "GÉMINIS",
            ["CAN"] = "CÁNCER",
            ["LEO"] = "LEO",
            ["VIR"] = "VIRGO",
            ["LIB"] = "LIBRA",
            ["ESC"] = "ESCORPIO",
            ["SAG"] = "SAGITARIO",
            ["CAP"] = "CAPRICORNIO",
            ["ACU"] = "ACUARIO",
            ["PIS"] = "PISCIS"
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
                var venezuelaNow = DateTime.Now;

                var loginResponse2 = await "https://carruselmillonario.com/juegos/api/getToken.php"
                    .PostJsonAsync(new
                    {
                        identidad = "plustaquilla",
                        credencial = "123123",
                        taquilla = 1769,
                        serial = "Integracion Premier"
                    })
                    .ReceiveString();
                var loginResponse = JObject.Parse(loginResponse2);
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

                if (!resultResponse.result.Where(x => !string.IsNullOrEmpty(x.numero)).Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(CarruselMillonario));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = resultResponse.result.Where(x => !string.IsNullOrEmpty(x.numero)).SelectMany(item =>
                {
                    var numbers = item.numero.Split('-');
                    var complements = item.resultado.Split('-');
                    string[] sorteosLabel = new string[] { "A", "B", "C" };
                    //var premierId = lotteries[hoursMap[time]];

                    return numbers.Select((x, i) =>
                    {
                        var time = item.sorteo.Replace("Carrusel M", "").Trim().ToUpper();
                        string resultado = "";
                        if (i < 2)
                        {
                            var number = animals[x].Item1;
                            var animal = animals[x].Item2;
                            resultado = number + " " + animal.Capitalize();
                        }
                        else {
                            resultado = sing[complements[i]];
                        }

                        time = hoursMap[time];
                        long? premierId = sorteosLabel[i] == "A" ? (lotteriesA[time]) : null;
                        return new Result
                        {
                            Result1 = resultado,
                            Time = time,
                            Date = DateTime.Now.ToString("dd-MM-yyyy"),
                            Sorteo = sorteosLabel[i],
                            ProductId = productID,
                            ProviderId = providerID,
                            ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                            PremierId = premierId,
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

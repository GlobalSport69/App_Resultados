using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using System.Collections.ObjectModel;

namespace LotteryResult.Services
{
    public class ChanceAnimalitosOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 16;
        private const int providerID = 16;
        private readonly ILogger<ChanceAnimalitosOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:00 AM", 262 },
            { "10:00 AM", 263 },
            { "11:00 AM", 264 },
            { "12:00 PM", 265 },
            { "01:00 PM", 266 },
            { "02:00 PM", 267 },
            { "03:00 PM", 268 },
            { "04:00 PM", 269 },
            { "05:00 PM", 270 },
            { "06:00 PM", 271 },
            { "07:00 PM", 272 }
        };

        public ChanceAnimalitosOfficial(IUnitOfWork unitOfWork, ILogger<ChanceAnimalitosOfficial> logger, INotifyPremierService notifyPremierService)
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

                var response = await "http://api.admfox.com.ve/Animalitos.svc/IAnimalitos/ListarResultados"
                    .AppendPathSegments(venezuelaNow.ToString("dd-MM-yyyy"), venezuelaNow.ToString("dd-MM-yyyy"))
                    .GetJsonAsync<List<ChanceAnimalitosResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(ChanceAnimalitosOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var substrings = item.fecSorteo.Split(' ');
                    var time = substrings[1].Substring(0, 2).Replace(":", "") + ":00 " + substrings[2];
                    time = LaGranjitaTerminalOfficial.FormatTime(time.ToUpper());
                    var animalFound = GetAnimalLabelFromNumber(item.codAnimalA);
                    var premierId = lotteries[time];

                    return new Result
                    {
                        Result1 = animalFound.Number + " " + animalFound.Name.Capitalize(),
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaGranjitaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ChanceAnimalitosOfficial));
                throw;
            }
        }

        private Animal GetAnimalLabelFromNumber(string number) {
            var animales = new Dictionary<string, Animal> {
                ["37"] = new Animal()
                {
                    Number = "0",
                    Name = "DELFIN",
                },
                ["38"] = new Animal()
                {
                    Number = "00",
                    Name = "BALLENA",
                },
                ["01"] = new Animal()
                {
                    Number = "01",
                    Name = "CARNERO",
                },

                ["02"] = new Animal()
                {
                    Number = "02",
                    Name = "TORO",
                },
                ["03"] = new Animal()
                {
                    Number = "03",
                    Name = "CIEMPIÉS",
                },
                ["04"] = new Animal()
                {
                    Number = "04",
                    Name = "ALACRÁN",
                },
                ["05"] = new Animal()
                {
                    Number = "05",
                    Name = "LEÓN",
                },
                ["06"] = new Animal()
                {
                    Number = "06",
                    Name = "RANA",
                },
                ["07"] = new Animal()
                {
                    Number = "07",
                    Name = "PERICO",
                },
                ["08"] = new Animal()
                {
                    Number = "08",
                    Name = "RATÓN",
                },
                ["09"] = new Animal()
                {
                    Number = "09",
                    Name = "ÁGUILA",
                },
                ["10"] = new Animal()
                {
                    Number = "10",
                    Name = "TIGRE",
                },
                ["11"] = new Animal()
                {
                    Number = "11",
                    Name = "GATO",
                },
                ["12"] = new Animal()
                {
                    Number = "12",
                    Name = "CABALLO",
                },
                ["13"] = new Animal()
                {
                    Number = "13",
                    Name = "MONO",
                },
                ["14"] = new Animal()
                {
                    Number = "14",
                    Name = "PALOMA",
                },
                ["15"] = new Animal()
                {
                    Number = "15",
                    Name = "ZORRO",
                },
                ["16"] = new Animal()
                {
                    Number = "16",
                    Name = "OSO",
                },
                ["17"] = new Animal()
                {
                    Number = "17",
                    Name = "PAVO",
                },
                ["18"] = new Animal()
                {
                    Number = "18",
                    Name = "BURRO",
                },
                ["19"] = new Animal()
                {
                    Number = "19",
                    Name = "CHIVO",
                },
                ["20"] = new Animal()
                {
                    Number = "20",
                    Name = "COCHINO",
                },
                ["21"] = new Animal()
                {
                    Number = "21",
                    Name = "GALLO",
                },
                ["22"] = new Animal()
                {
                    Number = "22",
                    Name = "CAMELLO",
                },
                ["23"] = new Animal()
                {
                    Number = "23",
                    Name = "CEBRA",
                },
                ["24"] = new Animal()
                {
                    Number = "24",
                    Name = "IGUANA",
                },
                ["25"] = new Animal()
                {
                    Number = "25",
                    Name = "GALLINA",
                },
                ["26"] = new Animal()
                {
                    Number = "26",
                    Name = "VACA",
                },
                ["27"] = new Animal()
                {
                    Number = "27",
                    Name = "PERRO",
                },
                ["28"] = new Animal()
                {
                    Number = "28",
                    Name = "ZAMURO",
                },
                ["29"] = new Animal()
                {
                    Number = "29",
                    Name = "ELEFANTE",
                },
                ["30"] = new Animal()
                {
                    Number = "30",
                    Name = "CAIMÁN",
                },
                ["31"] = new Animal()
                {
                    Number = "31",
                    Name = "LAPA",
                },
                ["32"] = new Animal()
                {
                    Number = "32",
                    Name = "ARDILLA",
                },
                ["33"] = new Animal()
                {
                    Number = "33",
                    Name = "PESCADO",
                },
                ["34"] = new Animal()
                {
                    Number = "34",
                    Name = "VENADO",
                },
                ["35"] = new Animal()
                {
                    Number = "35",
                    Name = "JIRAFA",
                },
                ["36"] = new Animal()
                {
                    Number = "36",
                    Name = "CULEBRA",
                },
            };
            if (!animales.ContainsKey(number))
                throw new Exception("Numero de animal invalido =>>> " + number);
            return animales[number];
        }
    }

    public class Animal {
        public string Number { get; set; }
        public string Name { get; set; }
    };

    public static class StringExtensions 
    { 
        public static string Capitalize(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("¡ARGUMENTO NO VÁLIDO!");

            return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
        }
    }
}

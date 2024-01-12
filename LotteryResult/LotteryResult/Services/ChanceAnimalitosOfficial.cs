using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using System.Collections.ObjectModel;

namespace LotteryResult.Services
{
    public class ChanceAnimalitosOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int chanceAnimalitosID = 16;
        private const int chanceAnimalitosProviderID = 16;
        private readonly ILogger<ChanceAnimalitosOfficial> _logger;

        public ChanceAnimalitosOfficial(IUnitOfWork unitOfWork, ILogger<ChanceAnimalitosOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                var venezuelaNow = DateTime.UtcNow.ToVenezuelaTimeZone();

                var response = await "http://api.admfox.com.ve/Animalitos.svc/IAnimalitos/ListarResultados"
                    .AppendPathSegments(venezuelaNow.ToString("dd-MM-yyyy"), venezuelaNow.ToString("dd-MM-yyyy"))
                    .GetJsonAsync<List<ChanceAnimalitosResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(ChanceAnimalitosOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == chanceAnimalitosProviderID &&
                        x.CreatedAt >= venezuelaNow.Date.ToUniversalTime());
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response)
                {
                    var substrings = item.fecSorteo.Split(' ');
                    var time = substrings[1].Substring(0, 5) + " " + substrings[2];
                    var animalFound = GetAnimalLabelFromNumber(item.codAnimalA);

                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = animalFound.Number +" "+ animalFound.Name.Capitalize(),
                        Time = time.ToUpper(),
                        Date = string.Empty,
                        ProductId = chanceAnimalitosID,
                        ProviderId = chanceAnimalitosProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS
                    });
                }

                Console.WriteLine(response);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ChanceAnimalitosOfficial));
                throw;
            }
        }

        private Animal GetAnimalLabelFromNumber(string number) {
            var Animales = new ObservableCollection<Animal>()
            {
                new Animal()
                {
                    Number = "37",
                    Name = "DELFIN",
                },
                new Animal()
                {
                    Number = "38",
                    Name = "BALLENA",
                },
                new Animal()
                {
                    Number = "01",
                    Name = "CARNERO",
                },
                new Animal()
                {
                    Number = "02",
                    Name = "TORO",
                },
                new Animal()
                {
                    Number = "03",
                    Name = "CIEMPIÉS",
                },
                new Animal()
                {
                    Number = "04",
                    Name = "ALACRÁN",
                },
                new Animal()
                {
                    Number = "05",
                    Name = "LEÓN",
                },
                new Animal()
                {
                    Number = "06",
                    Name = "RANA",
                },
                new Animal()
                {
                    Number = "07",
                    Name = "PERICO",
                },
                new Animal()
                {
                    Number = "08",
                    Name = "RATÓN",
                },
                new Animal()
                {
                    Number = "09",
                    Name = "ÁGUILA",
                },
                new Animal()
                {
                    Number = "10",
                    Name = "TIGRE",
                },
                new Animal()
                {
                    Number = "11",
                    Name = "GATO",
                },
                new Animal()
                {
                    Number = "12",
                    Name = "CABALLO",
                },
                new Animal()
                {
                    Number = "13",
                    Name = "MONO",
                },
                new Animal()
                {
                    Number = "14",
                    Name = "PALOMA",
                },
                new Animal()
                {
                    Number = "15",
                    Name = "ZORRO",
                },
                new Animal()
                {
                    Number = "16",
                    Name = "OSO",
                },
                new Animal()
                {
                    Number = "17",
                    Name = "PAVO",
                },
                new Animal()
                {
                    Number = "18",
                    Name = "BURRO",
                },
                new Animal()
                {
                    Number = "19",
                    Name = "CHIVO",
                },
                new Animal()
                {
                    Number = "20",
                    Name = "COCHINO",
                },
                new Animal()
                {
                    Number = "21",
                    Name = "GALLO",
                },
                new Animal()
                {
                    Number = "22",
                    Name = "CAMELLO",
                },
                new Animal()
                {
                    Number = "23",
                    Name = "CEBRA",
                },
                new Animal()
                {
                    Number = "24",
                    Name = "IGUANA",
                },
                new Animal()
                {
                    Number = "25",
                    Name = "GALLINA",
                },
                new Animal()
                {
                    Number = "26",
                    Name = "VACA",
                },
                new Animal()
                {
                    Number = "27",
                    Name = "PERRO",
                },
                new Animal()
                {
                    Number = "28",
                    Name = "ZAMURO",
                },
                new Animal()
                {
                    Number = "29",
                    Name = "ELEFANTE",
                },
                new Animal()
                {
                    Number = "30",
                    Name = "CAIMÁN",
                },
                new Animal()
                {
                    Number = "31",
                    Name = "LAPA",
                },
                new Animal()
                {
                    Number = "32",
                    Name = "ARDILLA",
                },
                new Animal()
                {
                    Number = "33",
                    Name = "PESCADO",
                },
                new Animal()
                {
                    Number = "34",
                    Name = "VENADO",
                },
                new Animal()
                {
                    Number = "35",
                    Name = "JIRAFA",
                },
                new Animal()
                {
                    Number = "36",
                    Name = "CULEBRA",
                },
            };

            var found = Animales.FirstOrDefault(x => x.Number == number);
            if (found is null)
            {
                throw new Exception("Numero de animal invalido =>>> "+ number);
            }
            return found;
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

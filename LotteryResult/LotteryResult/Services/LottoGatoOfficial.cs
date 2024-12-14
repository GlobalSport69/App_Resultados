using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Models.CarruselMillonario;
using LotteryResult.Models.Gatazo;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;
using System.Linq;

namespace LotteryResult.Services
{
    public class LottoGatoOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 36;
        private const int providerID = 35;
        private readonly ILogger<LottoGatoOfficial> _logger;
        private readonly INotifyPremierService notifyPremierService;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:10 AM", 334 },
            { "10:10 AM", 335 },
            { "11:10 AM", 336 },
            { "12:10 PM", 337 },
            { "01:10 PM", 338 },
            { "03:10 PM", 339 },
            { "04:10 PM", 340 },
            { "05:10 PM", 341 },
            { "06:10 PM", 342 },
            { "07:10 PM", 343 }
        };

        private Dictionary<string, string> hoursMap = new Dictionary<string, string>
        {
            { "O1", "09:10 AM" },
            { "O2", "10:10 AM" },
            { "O3", "11:10 AM"},
            { "O4", "12:10 PM" },
            { "O5", "01:10 PM" },
            { "O6", "03:10 PM" },
            { "O7", "04:10 PM" },
            { "O8", "05:10 PM" },
            { "O9", "06:10 PM" },
            { "1O", "07:10 PM" },
        };     

        public LottoGatoOfficial(IUnitOfWork unitOfWork, ILogger<LottoGatoOfficial> logger, INotifyPremierService notifyPremierService)
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

                var loginResponse2 = await "https://api.betm3.com/taquilla-api-v3/login"
                    .WithHeader("x-api-key", "11e78a1e-fe5f-46bb-a16a-09b6b07765b2")
                    .PostJsonAsync(new
                    {
                        u = "api-premier-plus",
                        p = "j3A21iMj8rr0",
                    })
                    .ReceiveString();
                var loginResponse = JObject.Parse(loginResponse2);

                JToken? status;
                if (!loginResponse.TryGetValue("s", out status) || !((bool)status))
                {
                    throw new Exception("Falta token "+ loginResponse);
                }

                var resultResponse = await "https://api.betm3.com/taquilla-api-v3/rani"
                    .WithHeader("x-api-key", "11e78a1e-fe5f-46bb-a16a-09b6b07765b2")
                    .SetQueryParams(new
                    {
                        tkn = loginResponse.SelectToken("a.tkn").ToString(),
                        f = (new DateTimeOffset(venezuelaNow)).ToUnixTimeMilliseconds()
                    })
                    .GetJsonAsync<Betm3Response>();

                if (!resultResponse.Status || !resultResponse.Data.Where(x => x.LotteryName.ToUpper() == "Lotto Gato".ToUpper()).Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LottoGatoOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = resultResponse.Data.FirstOrDefault(x => x.LotteryName.ToUpper() == "Lotto Gato".ToUpper()).Resultados.Select(item => {
                    var time = hoursMap[item.ID];
                    var number = item.Number;
                    //var animal = animals[item.Number];
                    //var resultado = number + " " + animal.Item2.Capitalize();
                    var animal = item.Animal;
                    var resultado = number + " " + animal.Capitalize();
                    var premierId = lotteries[time];

                    return new Result
                    {
                        Result1 = resultado,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = premierId,
                        //number = number,
                        //animal = animal
                    };
                }).OrderBy(x => x.Time)
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LottoGatoOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LottoGatoOfficial));
                throw;
            }
        }
    }
}

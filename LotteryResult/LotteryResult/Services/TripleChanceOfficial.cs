using Azure;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Models;
using PuppeteerSharp;
using System.Drawing;
using System.Linq;

namespace LotteryResult.Services
{
    public class TripleChanceOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 17;
        private const int providerID = 17;
        private readonly ILogger<TripleChanceOfficial> _logger;
        private INotifyPremierService notifyPremierService;

        private Dictionary<string, string> HoursMap = new Dictionary<string, string>
        {
            { "711", "01:00 PM" },
            { "712", "01:00 PM" },
            { "713", "01:00 PM" },
            { "721", "04:30 PM" },
            { "722", "04:30 PM" },
            { "723", "04:30 PM" },
            { "731", "07:00 PM" },
            { "732", "07:00 PM" },
            { "733", "07:00 PM" },
        };

        private Dictionary<string, string> LabelMap = new Dictionary<string, string>
        {
            { "711", "Triple A" },
            { "712", "Triple B" },
            { "713", "Chance Astral" },
            { "721", "Triple A" },
            { "722", "Triple B" },
            { "723", "Chance Astral" },
            { "731", "Triple A" },
            { "732", "Triple B" },
            { "733", "Chance Astral" },
        };

        private Dictionary<string, string> AstralMap = new Dictionary<string, string>
        {
            { "A", "ARIES" },
            { "B", "TAURO" },
            { "C", "GEMNIS" },
            { "D", "CANCER" },
            { "E", "LEO" },
            { "F", "VIRGO" },
            { "G", "LIBRA" },
            { "H", "ESCORPIO" },
            { "I", "SAGITARIO" },
            { "J", "CAPRICORNIO" },
            { "K", "ACUARIO" },
            { "L", "PISCIS" },
        };


        private Dictionary<string, long> Lotteries = new Dictionary<string, long>
        {
            { "711", 253 },
            { "721", 256 },
            { "731", 259 },
            { "712", 254 },
            { "722", 257 },
            { "732", 260 },
            { "713", 255 },
            { "723", 258 },
            { "733", 261 }
        };
        public TripleChanceOfficial(IUnitOfWork unitOfWork, ILogger<TripleChanceOfficial> logger, INotifyPremierService notifyPremierService)
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

                var response = await "https://api.loteriasoportex.com/monitor/resultados"
                   .PostJsonAsync(new
                   {
                       fecha = venezuelaNow.ToString("yyyy-MM-dd")
                   })
                .ReceiveJson<List<ChanceResponse>>();

                //response = response.Where(x => !x.Code.StartsWith('C')).ToList();
                response = response.Where(x => HoursMap.ContainsKey(x.Code)).ToList();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleChanceOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    //var time = item.Time.Substring(0, item.Time.Length - 2) + " " + item.Time.Substring(item.Time.Length - 2);
                    var time = HoursMap[item.Code];
                    var sorteo = LabelMap[item.Code];
                    long premierId = Lotteries[item.Code];

                    var resultado = item.Number;
                    if (resultado.Length > 3)
                    {
                        var astralKey = new string(resultado.TakeLast(1).ToArray());
                        resultado = new string(resultado.Take(3).ToArray()) + " " + AstralMap[astralKey];
                    }

                    return new Result
                    {
                        Result1 = resultado,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = sorteo,
                        PremierId = premierId,
                        //Number: item.Result,
                        //Complement: complement
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleChanceOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleChanceOfficial));
                throw;
            }
        }
    }
}


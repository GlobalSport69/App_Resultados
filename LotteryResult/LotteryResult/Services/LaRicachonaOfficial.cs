using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaRicachonaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 12;
        private const int providerID = 12;
        private readonly ILogger<LaRicachonaOfficial> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:05 AM", 90 },
            { "10:05 AM", 91 },
            { "11:05 AM", 92 },
            { "12:05 PM", 93 },
            { "01:05 PM", 94 },
            { "02:05 PM", 95 },
            { "03:05 PM", 96 },
            { "04:05 PM", 97 },
            { "05:05 PM", 98 },
            { "06:05 PM", 99 },
            { "07:05 PM", 100 }
        };
        public LaRicachonaOfficial(IUnitOfWork unitOfWork, ILogger<LaRicachonaOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var response = await "https://webservice.premierpluss.com"
                .AppendPathSegments("loteries", "results3")
                .SetQueryParams(new
                {
                    since = venezuelaNow.ToString("yyyy-MM-dd"),
                    product = 9
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaRicachonaOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.lottery.name.Replace("LA RICACHONA ", "").Replace("O", "0").ToUpper();
                    time = LaGranjitaTerminalOfficial.FormatTime(time);
                    var PremierId = lotteries[time];


                    return new Result
                    {
                        Result1 = item.result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        PremierId = PremierId
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
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaRicachonaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaRicachonaOfficial));
                throw;
            }
        }
    }
}

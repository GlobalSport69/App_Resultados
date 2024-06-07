using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using Microsoft.CodeAnalysis;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaGranjitaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 1;
        private const int providerID = 3;
        private readonly ILogger<LaGranjitaOfficial> _logger;

        //private Dictionary<string, long> lotteries = new Dictionary<string, long>
        //{
        //    { "08:00 AM", 239 },
        //    { "09:00 AM", 2 },
        //    { "10:00 AM", 3 },
        //    { "11:00 AM", 4},
        //    { "12:00 PM", 5 },
        //    { "01:00 PM", 6 },
        //    { "02:00 PM", 7 },
        //    { "03:00 PM", 8 },
        //    { "04:00 PM", 9 },
        //    { "05:00 PM", 10 },
        //    { "06:00 PM", 11 },
        //    { "07:00 PM", 12 }
        //};

        public LaGranjitaOfficial(IUnitOfWork unitOfWork, ILogger<LaGranjitaOfficial> logger)
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
                    product = 1
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaGranjitaOfficial));
                    return; 
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var sorteos = await unitOfWork.LotteryRepository
                    .GetAllByAsync(x => x.ProductId == productID);
                var lotteries = sorteos.ToDictionary(x => x.LotteryHour.FormatTime());

                var newResult = response.Select(item => {
                    var time = item.lottery.name.Replace("LA GRANJITA ", "").Replace("O", "0").ToUpper();
                    time = LaGranjitaTerminalOfficial.FormatTime(time);

                    var sorteo = lotteries[time];
                    var result = item.result.Split("-");
                    var number = result[0];
                    var animal = result[1];

                    return new Result
                    {
                        Result1 = number + " " + animal,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,

                        PremierId = sorteo.PremierId,
                        LotteryId = sorteo.Id,
                        Number = number,
                        Animal = animal
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

                if (needSave) { 
                    await unitOfWork.SaveChangeAsync();
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaGranjitaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaGranjitaOfficial));
                throw;
            }
        }
    }
}

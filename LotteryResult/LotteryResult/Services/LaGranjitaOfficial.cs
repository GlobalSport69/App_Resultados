using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaGranjitaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int laGranjitaID = 1;
        private const int laGranjitaProviderID = 3;
        private readonly ILogger<LaGranjitaOfficial> _logger;

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

                var lotteries = await unitOfWork.LotteryRepository.GetAllByAsync(x => x.ProductId == laGranjitaID);

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == laGranjitaProviderID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.lottery.name.Replace("LA GRANJITA ", "").Replace("O", "0").ToUpper();
                    var lottery = lotteries.FirstOrDefault(x => x.LotteryHour == LaGranjitaTerminalOfficial.ConvertToTime(time));

                    return new Result
                    {
                        Result1 = item.result.Replace("-", " "),
                        Time = LaGranjitaTerminalOfficial.FormatTime(time),
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = laGranjitaID,
                        ProviderId = laGranjitaProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        LotteryId = lottery is null ? 0 : lottery.Id,
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

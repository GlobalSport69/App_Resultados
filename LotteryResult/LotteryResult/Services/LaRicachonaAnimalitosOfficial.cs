using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services
{
    public class LaRicachonaAnimalitosOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int laRicachonaAnimalitosID = 14;
        private const int laRicachonaAnimalitosProviderID = 14;
        private readonly ILogger<LaRicachonaAnimalitosOfficial> _logger;
        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:00 AM", 167 },
            { "10:00 AM", 169 },
            { "11:00 AM", 170 },
            { "12:00 PM", 171 },
            { "01:00 PM", 172 },
            { "02:00 PM", 173 },
            { "03:00 PM", 174 },
            { "04:00 PM", 175 },
            { "05:00 PM", 176 },
            { "06:00 PM", 177 },
            { "07:00 PM", 178 }
        };
        public LaRicachonaAnimalitosOfficial(IUnitOfWork unitOfWork, ILogger<LaRicachonaAnimalitosOfficial> logger)
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
                    product = 16
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaRicachonaAnimalitosOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == laRicachonaAnimalitosProviderID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = item.lottery.name.Replace("ANIMALITOS LA RICACHONA ", "").Replace("O", "0").ToUpper();
                    var PremierId = lotteries[LaGranjitaTerminalOfficial.FormatTime(time)];

                    return new Result
                    {
                        Result1 = item.result.Replace("-", " "),
                        Time = LaGranjitaTerminalOfficial.FormatTime(time),
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = laRicachonaAnimalitosID,
                        ProviderId = laRicachonaAnimalitosProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = PremierId
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();

                var toUpdate = new List<Result>();
                foreach (var item in newResult)
                {
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Result1 != y.Result1);
                    if (found is null)
                        continue;

                    found.Result1 = item.Result1;
                    toUpdate.Add(found);
                }
                var toInsert = newResult.Where(x => !oldResult.Exists(y => x.Time == y.Time));
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LaRicachonaAnimalitosOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaRicachonaAnimalitosOfficial));
                throw;
            }
        }
    }
}

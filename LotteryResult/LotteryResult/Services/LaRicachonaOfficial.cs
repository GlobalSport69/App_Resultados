using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class LaRicachonaOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int laRicachonaID = 13;
        private const int laRicachonaProviderID = 13;
        private readonly ILogger<LaRicachonaOfficial> _logger;

        public LaRicachonaOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<LaRicachonaOfficial> logger)
        {
            this.resultRepository = resultRepository;
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                // Obtén la zona horaria de Venezuela
                TimeZoneInfo venezuelaZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");

                // Obtén la fecha y hora actual en UTC
                DateTime utcNow = DateTime.UtcNow;

                // Convierte la fecha y hora actual a la zona horaria de Venezuela
                DateTime venezuelaNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, venezuelaZone);

                var response = await "https://webservice.premierpluss.com"
                .AppendPathSegments("loteries", "results3")
                .SetQueryParams(new
                {
                    since = venezuelaNow.ToString("yyyy-MM-dd"),
                    product = 9
                })
                .GetJsonAsync<List<GetLaGranjitaOfficialResponse>>();

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == laRicachonaProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in response)
                {
                    resultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.result,
                        Time = item.lottery.name.Replace("LA RICACHONA ", "").Replace("O", "0").ToUpper(),
                        Date = string.Empty,
                        ProductId = laRicachonaID,
                        ProviderId = laRicachonaProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES
                    });
                }

                //Console.WriteLine(response);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(LaRicachonaOfficial));
                throw;
            }
        }
    }
}

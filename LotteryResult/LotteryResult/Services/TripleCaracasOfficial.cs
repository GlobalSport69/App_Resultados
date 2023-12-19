using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class TripleCaracasOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int tripleCaracasID = 3;
        private const int tripleCaracasProviderID = 8;
        private readonly ILogger<TripleCaracasOfficial> _logger;

        public TripleCaracasOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<TripleCaracasOfficial> logger)
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


                var response = await "http://165.227.185.69:8000"
                .AppendPathSegments("api", "resultados")
                .SetQueryParams(new
                {
                    fecha = venezuelaNow.ToString("yyyy-MM-dd"),
                })
                .GetJsonAsync<TripleCaracasOfficialResponse>();

                var results = response.resultados
                    .GroupBy(x => x.producto_juego.order)
                    .OrderBy(x => x.Key)
                    .TakeWhile(x => x.Key < 3)
                    .SelectMany(x => x)
                    .ToList();

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == tripleCaracasProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in results)
                {
                    resultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.resultado,
                        Time = item.sorteo.hora,
                        Date = string.Empty,
                        ProductId = tripleCaracasID,
                        ProviderId = tripleCaracasProviderID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = item.producto_juego.nombre
                    });
                }

                Console.WriteLine(response);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleCaracasOfficialResponse));
                throw;
            }
        }
    }
}

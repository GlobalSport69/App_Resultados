using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;
using System.Globalization;

namespace LotteryResult.Services
{
    public class TripleCaracasOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int tripleCaracasID = 3;
        private const int tripleCaracasProviderID = 9;
        private readonly ILogger<TripleCaracasOfficial> _logger;

        public TripleCaracasOfficial(IUnitOfWork unitOfWork, ILogger<TripleCaracasOfficial> logger)
        {
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

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == tripleCaracasProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in results)
                {
                    // Crear un DateTime a partir de la cadena de texto "13:00:00"
                    DateTime dt = DateTime.ParseExact(item.sorteo.hora, "HH:mm:ss", CultureInfo.InvariantCulture);
                    // Convertir a formato de 12 horas (AM/PM)
                    string time12Hour = dt.ToString("hh:mm tt", CultureInfo.InvariantCulture);


                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.resultado,
                        Time = time12Hour,
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

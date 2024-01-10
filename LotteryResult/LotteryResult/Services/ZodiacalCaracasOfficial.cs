using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using System.Globalization;

namespace LotteryResult.Services
{
    public class ZodiacalCaracasOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 18;
        private const int providerID = 18;
        private readonly ILogger<ZodiacalCaracasOfficial> _logger;

        public ZodiacalCaracasOfficial(IUnitOfWork unitOfWork, ILogger<ZodiacalCaracasOfficial> logger)
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
                    .Where(x => x.Key == 3)
                    .SelectMany(x => x)
                    .ToList();

                if (!results.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(ZodiacalCaracasOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID &&
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
                        Result1 = item.resultado +" "+item.resultado_elemento,
                        Time = time12Hour,
                        Date = string.Empty,
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.ZODIACAL,
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(ZodiacalCaracasOfficial));
                throw;
            }
        }
    }
}
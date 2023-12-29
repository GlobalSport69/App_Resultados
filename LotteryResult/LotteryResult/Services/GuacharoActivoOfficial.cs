using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class GuacharoActivoOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        public const int guacharoID = 11;
        private const int guacharoProviderID = 11;
        private readonly ILogger<GuacharoActivoOfficial> _logger;

        public GuacharoActivoOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<GuacharoActivoOfficial> logger)
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


                var response = await "https://www.apuestanext.com/apuestanext.com/aplicativo/accion/apis/resultados.php"
                .PostJsonAsync(new {
                    id_tipo_loteria = 96,
                    fecha = venezuelaNow.ToString("yyyy-MM-dd")
                })
                .ReceiveJson<List<GuacharoOfficialResponse>>();

                if (!response.Any()) return;

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == guacharoProviderID &&
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
                        Result1 = item.numero + " " +item.nombre.Trim(),
                        Time = item.loteria.Replace("Guacharo Activo ", "").ToUpper(),
                        Date = string.Empty,
                        ProductId = guacharoID,
                        ProviderId = guacharoProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALES77
                    });
                }

                Console.WriteLine(response);

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(GuacharoActivoOfficial));
                throw;
            }
        }
    }
}

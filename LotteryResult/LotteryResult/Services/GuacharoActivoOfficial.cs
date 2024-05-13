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
        private IUnitOfWork unitOfWork;
        public const int guacharoID = 11;
        private const int guacharoProviderID = 11;
        private readonly ILogger<GuacharoActivoOfficial> _logger;

        public GuacharoActivoOfficial(IUnitOfWork unitOfWork, ILogger<GuacharoActivoOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var response = await "https://www.apuestanext.com/apuestanext.com/aplicativo/accion/apis/resultados.php"
                .PostJsonAsync(new {
                    id_tipo_loteria = 96,
                    fecha = venezuelaNow.ToString("yyyy-MM-dd")
                })
                .ReceiveJson<List<GuacharoOfficialResponse>>();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(GuacharoActivoOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == guacharoProviderID && x.CreatedAt.Date == venezuelaNow.Date);
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response)
                {
                    unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.numero + " " +item.nombre.Trim(),
                        Time = item.loteria.Replace("Guacharo Activo ", "").ToUpper(),
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
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

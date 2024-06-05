using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LotteryResult.Services
{
    public class BanklotResponse
    {
        public string hora_sorteo { get; set; }
        public string fecha_sorteo { get; set; }
        public List<string> results { get; set; }
    }
    public class TripleZamoranoBanklot : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 2;
        private const int providerID = 1;
        private readonly ILogger<TripleZamoranoBanklot> _logger;

        public TripleZamoranoBanklot(IUnitOfWork unitOfWork, ILogger<TripleZamoranoBanklot> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var response = await "https://api.banklot.net/api/result/lottery"
                .PostJsonAsync(new
                {
                    loteria = "4",
                    producto = "2"
                })
                .ReceiveJson<List<BanklotResponse>>();

                response = response.Where(x => x.fecha_sorteo == venezuelaNow.ToString("dd/MM/yyyy")).ToList();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleZamoranoBanklot));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                foreach (var item in oldResult)
                {
                    unitOfWork.ResultRepository.Delete(item);
                }

                foreach (var item in response.SelectMany(x => x.results.Select(y => (new {
                    numero = y,
                    time = x.hora_sorteo,
                }))))
                {
                    //unitOfWork.ResultRepository.Insert(new Data.Models.Result
                    //{
                    //    Result1 = item.numero + " " + item.nombre.Trim(),
                    //    Time = item.loteria.Replace("Guacharo Activo ", "").ToUpper(),
                    //    Date = string.Empty,
                    //    ProductId = productID,
                    //    ProviderId = providerID,
                    //    ProductTypeId = (int)ProductTypeEnum.TRIPLES
                    //});
                }

                Console.WriteLine(response);

                //await unitOfWork.SaveChangeAsync();


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleZamoranoBanklot));
                throw;
            }
        }
    }
}

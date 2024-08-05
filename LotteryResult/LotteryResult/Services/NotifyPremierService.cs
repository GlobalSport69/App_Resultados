using Flurl;
using Flurl.Http;
using Hangfire;
using LotteryResult.Data.Abstractions;
using LotteryResult.Enum;
using System.Globalization;

namespace LotteryResult.Services
{
    public class PremicionPremierDto
    {
        public long lotteryId { get; set; }
        public string number { get; set; }
        public string date { get; set; }
        public string? complement_number { get; set; }
    }
    
    public enum NotifyType { 
        New,
        Update
    }
    public interface INotifyPremierService {
        public void Handler(List<long> result_ids, NotifyType type = NotifyType.New);
    }
    public class NotifyPremierService : INotifyPremierService
    {
        private IUnitOfWork unitOfWork;
        private ILogger<NotifyPremierService> _logger;
        private IConfiguration _configuration;

        public NotifyPremierService(IUnitOfWork unitOfWork, ILogger<NotifyPremierService> logger, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
            _configuration = configuration;
        }


        public void Handler(List<long> result_ids, NotifyType type = NotifyType.New)
        {
            BackgroundJob.Enqueue("notify_premier", () => EnvioTelegram(result_ids, type));
            BackgroundJob.Enqueue("notify_premier", () => Premiacion(result_ids, type));
        }

        public async Task EnvioTelegram(List<long> result_ids, NotifyType type)
        {
            try
            {
                var results = await unitOfWork.ResultRepository.GetResultByIds(result_ids);
                var found = results.GroupBy(x => x.Product.Name).FirstOrDefault();
                if (found is null)
                    return;
                //var message = string.Empty;
                var message = "PRODUCCION\n";
                if (type == NotifyType.New)
                    message += "NUEVOS\n";
                else 
                    message += "ACTUALIZADOS\n";

                message += "\n" + found.Key + "\n";
                foreach (var item in found)
                {
                    message += "Resultado: " + item.Result1 + "\n" + "Hora: " + item.Time + "\n" + "Sorteo: " + (string.IsNullOrEmpty(item.Sorteo) ? "N/A" : item.Sorteo) + "\n";
                }

                var result = await "https://api.telegram.org/bot6844458606:AAGYYpQDieh-sv-gyjGXBVd1mhQoiTqQ-2I/sendMessage"
                    .SetQueryParams(new {
                        chat_id= "-1002082761148",
                        parse_mode= "html",
                        text= message,
                        reply_to_message_id = "207712"
                    })
                    .GetStringAsync();
            }
            catch (FlurlHttpException fex)
            {

                throw;
            }
        }
        public async Task Premiacion(List<long> result_ids, NotifyType type)
        {
            PremicionPremierDto body = null;
            try
            {
                var results = await unitOfWork.ResultRepository.GetResultByIds(result_ids);

                // la linea final es esta
                //var found = results.GroupBy(x => x.Product.Name).FirstOrDefault();

                // esta line es provicional
                var tripleChanceID = 17;
                var found = results.GroupBy(x => x.Product.Id).FirstOrDefault(g => g.Key == tripleChanceID);
                
                
                if (found is null)
                    return;

                foreach (var item in found.Where(r => r.PremierId.HasValue))
                {
                    var number = new string(item.Result1.TakeWhile(c => c != ' ').ToArray());
                    var date = DateOnly.ParseExact(item.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                    var animals = new int[] { (int)ProductTypeEnum.ANIMALITOS, (int)ProductTypeEnum.ANIMALES77 };
                    if (animals.Contains(item.ProductTypeId.Value))
                        number = number == "00" ? number : int.Parse(number).ToString();

                    body = new PremicionPremierDto
                    {
                        lotteryId = item.PremierId.Value,
                        number = number,
                        date = date,
                        complement_number = null
                    };
                    var hookUrl = _configuration.GetSection("PremierHookUrl").Value ?? string.Empty;
                    if (string.IsNullOrEmpty(hookUrl))
                        throw new ArgumentException("PremierHookUrl is empty");

                    var result = await hookUrl
                    .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36")
                    .WithHeader("Referer", hookUrl)
                    .PostJsonAsync(body)
                    .ReceiveString();


                    using (_logger.BeginScope(new Dictionary<string, object>{
                        { Serilog.Core.Constants.SourceContextPropertyName, typeof(NotifyPremierService).FullName }
                    }))
                    {
                        _logger.LogInformation(@"Respuesta obtenida: {0} Request: {@body}", result, body);
                    }

                    await Task.Delay(1000);
                }
            }
            catch (FlurlHttpException ex)
            {
                var response = await ex.GetResponseStringAsync();

                using (_logger.BeginScope(new Dictionary<string, object>{
                        { Serilog.Core.Constants.SourceContextPropertyName, typeof(NotifyPremierService).FullName }
                    }))
                {
                    _logger.LogError(exception: ex, message: "Request: {@body} Response: {response}", body, response);
                }
                throw;
            }
        }
    }
}

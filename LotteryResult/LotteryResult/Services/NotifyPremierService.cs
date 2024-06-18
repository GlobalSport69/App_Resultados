using Flurl;
using Flurl.Http;
using Hangfire;
using LotteryResult.Data.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace LotteryResult.Services
{
    
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

        public NotifyPremierService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }


        public void Handler(List<long> result_ids, NotifyType type = NotifyType.New)
        {
            BackgroundJob.Enqueue("notify_premier", () => LongTask(result_ids, type));
        }

        public async Task LongTask(List<long> result_ids, NotifyType type)
        {
            try
            {
                var results = await unitOfWork.ResultRepository.GetResultByIds(result_ids);
                var found = results.GroupBy(x => x.Product.Name).FirstOrDefault();
                if (found is null)
                    return;

                var message = string.Empty;
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

            //var init = DateTime.Now;
            //await Task.Delay(TimeSpan.FromSeconds(tick * 60));
            //var end = DateTime.Now;
            
            //try
            //{
            //    using (StreamWriter writer = new StreamWriter("Archivo.txt", true))
            //    {
            //        writer.WriteLine("--------------------------------------");
            //        writer.WriteLine("Escribiendo en el archivo..." + init);
            //        writer.WriteLine("Escribiendo en el archivo..." + end);
            //        writer.WriteLine("Escribiendo en el archivo..." + tick+" "+ (end - init).Seconds);
            //    }
            //}
            //catch (IOException e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }
    }
}

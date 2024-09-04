using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services.CloseNotification
{
    public class Guacharo : IGetResult
    {
        private readonly SendCloseLotteryNotifyToPremierApi _notifyClose;

        public Guacharo(SendCloseLotteryNotifyToPremierApi notifyClose)
        {
            _notifyClose = notifyClose;
        }

        public const string CronExpression = "55 7,8,9,10,11,12,13,14,15,16,17,18 * * *";
        public const int productID = 11;

        private Dictionary<int, long> lotteries = new Dictionary<int, long>
        {
            { 8, 287 },
            { 9, 228 },
            { 10, 229 },
            { 11, 230 },
            { 12, 231 },
            { 13, 232 },
            { 14, 233 },
            { 15, 234 },
            { 16, 235 },
            { 17, 236 },
            { 18, 237 },
            { 19, 238 }
        };

        public async Task Handler()
        {
            var venezuelaNow = DateTime.Now;
            if (lotteries.ContainsKey(DateTime.Now.AddMinutes(10).Hour))
            {
                await _notifyClose.Handler(lotteries[DateTime.Now.AddMinutes(10).Hour]);
            }
        }
    }
}
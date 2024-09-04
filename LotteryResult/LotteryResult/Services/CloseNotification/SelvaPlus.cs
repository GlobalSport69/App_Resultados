using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;

namespace LotteryResult.Services.CloseNotification
{
    public class SelvaPlus : IGetResult
    {
        private readonly SendCloseLotteryNotifyToPremierApi _notifyClose;

        public SelvaPlus(SendCloseLotteryNotifyToPremierApi notifyClose)
        {
            _notifyClose = notifyClose;
        }
        public const string CronExpression = "55 7,8,9,10,11,12,13,14,15,16,17,18 * * *";
        public const int productID = 10;

        private Dictionary<int, long> lotteries = new Dictionary<int, long>
        {
            { 8, 288 },
            { 9, 207 },
            { 10, 208 },
            { 11, 209 },
            { 12, 210 },
            { 13, 211 },
            { 14, 212 },
            { 15, 213 },
            { 16, 214 },
            { 17, 215 },
            { 18, 216 },
            { 19, 217 }
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
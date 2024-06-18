using LotteryResult.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Abstractions
{
    public interface IResultRepository : IGenericRepository<Result>
    {
        public Task<List<Result>> GetResultByIds(List<long> resultID);
    }
}

using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using LotteryResult.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Implementations
{
    internal class ResultRepository : GenericRepository<Result>, IResultRepository
    {
        public ResultRepository(PostgresDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<List<Result>> GetResultByIds(List<long> resultID)
        {
            return await _dbContext.Results
                .Where(r => resultID.Contains(r.Id))
                .Include(x => x.Product)
                .ToListAsync();
        }
    }
}

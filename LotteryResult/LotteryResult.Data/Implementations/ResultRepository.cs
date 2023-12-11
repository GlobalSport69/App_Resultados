using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using LotteryResult.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Implementations
{
    internal class ResultRepository : GenericRepository<Result>, IResultRepository
    {
        public ResultRepository(PostgresDbContext dbContext) : base(dbContext)
        {
        }
    }
}

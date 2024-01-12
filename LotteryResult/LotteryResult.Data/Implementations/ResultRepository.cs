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

        public async Task DeleteResultByDateAsync(DateTime date, Expression<Func<Result, bool>> expression = null)
        {
            CultureInfo cultureInfo = new CultureInfo("es-VE");

            DateTime inicioDelDia = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, cultureInfo.Calendar);
            DateTime inicioDelDiaUTC = inicioDelDia.ToUniversalTime();

            DateTime finDelDia = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, cultureInfo.Calendar);
            DateTime finDelDiaUTC = finDelDia.ToUniversalTime();

            var query = _dbContext.Results.Where(r => r.CreatedAt >= inicioDelDiaUTC && r.CreatedAt <= finDelDiaUTC);
            if (expression != null)
            {
                query.Where(expression);
            }

            var oldResult = await query.ToListAsync();

            foreach (var item in oldResult)
            {
                _dbContext.Results.Remove(item);
            }
        }
    }
}

using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using LotteryResult.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Implementations
{
    internal class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(PostgresDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Product>> GetResultByProductsByDate(DateTime date)
        {
            DateTime inicioDelDia = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTime inicioDelDiaUTC = inicioDelDia.ToUniversalTime();

            DateTime finDelDia = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            DateTime finDelDiaUTC = finDelDia.ToUniversalTime();

            return await _dbContext.Products
                .Where(x => x.Enable)
                .Include(x => x.Results.Where(r => r.CreatedAt.ToUniversalTime() >= inicioDelDiaUTC && 
                                                   r.CreatedAt.ToUniversalTime() <= finDelDiaUTC))
                .ToListAsync();
        }
    }
}

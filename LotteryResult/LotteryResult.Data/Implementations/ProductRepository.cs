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
    internal class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(PostgresDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Product>> GetResultByProductsByDate(DateTime date)
        {
            return await _dbContext.Products
                .Where(x => x.Enable)
                .Include(x => x.Results.Where(r => r.CreatedAt.Date == date))
                .ToListAsync();
        }
    }
}

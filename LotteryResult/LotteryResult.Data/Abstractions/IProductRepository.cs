using LotteryResult.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Abstractions
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        Task<List<Product>> GetResultByProductsByDate(DateTime dateTime);
        Task<List<Product>> GetResultByProductsByRangeDate(DateTime from, DateTime until);

    }
}

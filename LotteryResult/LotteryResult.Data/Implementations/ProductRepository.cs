﻿using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using LotteryResult.Data.Models;
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
    }
}

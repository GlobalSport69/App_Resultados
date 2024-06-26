﻿using LotteryResult.Data.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Abstractions
{
    public interface IUnitOfWork
    {
        public IResultRepository ResultRepository { get; }
        public IProductRepository ProductRepository { get; }
        public ILotteryRepository LotteryRepository { get; }
        public Task<int> SaveChangeAsync();
    }
}

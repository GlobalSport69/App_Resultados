using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Implementations
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly PostgresDbContext _dbContext;

        private IResultRepository resultRepository;
        public IResultRepository ResultRepository
        {
            get
            {
                if (this.resultRepository == null)
                {
                    this.resultRepository = new ResultRepository(_dbContext);
                }
                return resultRepository;
            }
        }
        public IProductRepository productRepository;
        public IProductRepository ProductRepository
        {
            get
            {
                if (this.productRepository == null)
                {
                    this.productRepository = new ProductRepository(_dbContext);
                }
                return productRepository;
            }
        }

        private ILotteryRepository lotteryRepository;
        public ILotteryRepository LotteryRepository
        {
            get
            {
                if (this.lotteryRepository == null)
                {
                    this.lotteryRepository = new LotteryRepository(_dbContext);
                }
                return lotteryRepository;
            }
        }

        public UnitOfWork(PostgresDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        //private IDbContextTransaction _transaction;

        //public void BeginTransaction() {
        //    _transaction = _dbContext.Database.BeginTransaction();
        //}

        //public void Commit()
        //{
        //    _transaction.Commit();
        //}

        //public void Rollback()
        //{
        //    _transaction.Rollback();
        //}
    }
}

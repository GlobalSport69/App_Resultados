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
        private IDbContextTransaction _transaction;

        public UnitOfWork(PostgresDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void BeginTransaction() {
            _transaction = _dbContext.Database.BeginTransaction();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }
    }
}

using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LotteryResult.Data.Implementations
{
    internal class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public PostgresDbContext _dbContext;

        public GenericRepository(PostgresDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllByAsync(Expression<Func<T, bool>> predicate = null, IEnumerable<string> includes = null)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (includes != null && includes.Any())
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }

            if (predicate is null)
                return await query.ToListAsync();

            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T> GetByAsync(Expression<Func<T, bool>> predicate, IEnumerable<string> includes = null)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (includes != null && includes.Any())
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }

            return await query.Where(predicate).FirstOrDefaultAsync();
        }

        public void Insert(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }


        public void Insert(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().AddRange(entities);
        }

        //public async Task<int> SaveChangeAsync()
        //{
        //    return await _writer.SaveChangesAsync();
        //}

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }
    }
}

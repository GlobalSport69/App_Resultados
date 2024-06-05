using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Abstractions
{
    public interface IGenericRepository<T>
    {
        public void Insert(T entity);
        public void Insert(IEnumerable<T> entities);
        public void Update(T entity);
        public void Delete(T entity);
        public Task<T> GetByAsync(Expression<Func<T, bool>> predicate, IEnumerable<string> includes = null);
        public Task<List<T>> GetAllByAsync(Expression<Func<T, bool>> predicate = null, IEnumerable<string> includes = null);
        //public Task<int> SaveChangeAsync();
    }
}

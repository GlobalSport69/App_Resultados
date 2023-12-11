using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data.Abstractions
{
    public interface IUnitOfWork
    {
        public Task<int> SaveChangeAsync();
    }
}

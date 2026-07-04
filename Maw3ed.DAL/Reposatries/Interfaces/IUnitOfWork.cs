using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
      
        IAuthRepository AuthRepository { get; }
        Task<int> SaveChangesAsync();
    }
}

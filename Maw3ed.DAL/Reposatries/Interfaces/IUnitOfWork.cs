using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync();
    }
}
